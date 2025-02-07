using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Dto.Response.Documents;
using Common.Logging;
using Common.Telemetry;
using coordinator.Constants;
using coordinator.Domain.Exceptions;
using coordinator.Durable.Activity;
using coordinator.Durable.Payloads;
using coordinator.Durable.Payloads.Domain;
using coordinator.TelemetryEvents;
using coordinator.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Common.Domain.Document;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using coordinator.Domain;

namespace coordinator.Durable.Orchestration
{
    public class RefreshCaseOrchestrator
    {
        private readonly ILogger<RefreshCaseOrchestrator> _log;
        private readonly IConfiguration _configuration;
        private readonly ICmsDocumentsResponseValidator _cmsDocumentsResponseValidator;
        private readonly ITelemetryClient _telemetryClient;
        private readonly TimeSpan _timeout;

        public RefreshCaseOrchestrator(
            ILogger<RefreshCaseOrchestrator> log,
            IConfiguration configuration,
            ICmsDocumentsResponseValidator cmsDocumentsResponseValidator,
            ITelemetryClient telemetryClient)
        {
            _log = log;
            _configuration = configuration;
            _cmsDocumentsResponseValidator = cmsDocumentsResponseValidator;
            _telemetryClient = telemetryClient;
            _timeout = TimeSpan.FromSeconds(double.Parse(_configuration[ConfigKeys.CoordinatorOrchestratorTimeoutSecs]));
        }

        [Function(nameof(RefreshCaseOrchestrator))]
        public async Task Run([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var payload = context.GetInput<CasePayload>()
                ?? throw new ArgumentException("Orchestration payload cannot be null.", nameof(context));

            var log = context.CreateReplaySafeLogger(nameof(RefreshCaseOrchestrator));

            RefreshedCaseEvent telemetryEvent = default;
            try
            {
                await context.CallActivityAsync(nameof(SetCaseStatus), new SetCaseStatusPayload { CaseId = payload.CaseId, UpdatedAt = context.CurrentUtcDateTime, Status = CaseRefreshStatus.Running, FailedReason = null });

                telemetryEvent = new RefreshedCaseEvent(
                    correlationId: payload.CorrelationId,
                    caseId: payload.CaseId,
                    startTime: await context.CallActivityAsync<DateTime>(nameof(GetStartTime), payload.CaseId))
                {
                    OperationName = nameof(RefreshCaseOrchestrator),
                };

                var orchestratorTask = RunCaseOrchestrator(context, payload, telemetryEvent, log);

                using var cts = new CancellationTokenSource();
                var deadline = context.CurrentUtcDateTime.Add(_timeout);
                var timeoutTask = context.CreateTimer(deadline, cts.Token);

                var result = await Task.WhenAny(orchestratorTask, timeoutTask);
                if (result == orchestratorTask)
                {
                    // success case
                    cts.Cancel();
                    _telemetryClient.TrackEvent(telemetryEvent);
                    await orchestratorTask;
                    return;
                }

                throw new TimeoutException($"Orchestration with id '{context.InstanceId}' timed out.");
            }
            catch (Exception exception)
            {
                await context.CallActivityAsync(nameof(SetCaseStatus), new SetCaseStatusPayload { CaseId = payload.CaseId, UpdatedAt = context.CurrentUtcDateTime, Status = CaseRefreshStatus.Failed, FailedReason = exception.Message });

                log.LogMethodError(payload.CorrelationId, nameof(RefreshCaseOrchestrator), $"Error when running {nameof(RefreshCaseOrchestrator)} orchestration with id '{context.InstanceId}'", exception);
                _telemetryClient.TrackEventFailure(telemetryEvent);
                throw;
            }
        }

        private async Task RunCaseOrchestrator(TaskOrchestrationContext context, CasePayload payload, RefreshedCaseEvent telemetryEvent, ILogger logger)
        {
            await context.CallActivityAsync(nameof(Reset), payload.CaseId);
            await context.CallActivityAsync(nameof(SetCaseStatus), new SetCaseStatusPayload { CaseId = payload.CaseId, UpdatedAt = context.CurrentUtcDateTime, Status = CaseRefreshStatus.Running, FailedReason = null });

            var documents = await GetDocuments(context, payload);
            telemetryEvent.CmsDocsCount = documents.CmsDocuments.Length;
            await context.CallActivityAsync<bool>(nameof(SetCaseStatus), new SetCaseStatusPayload { CaseId = payload.CaseId, UpdatedAt = context.CurrentUtcDateTime, Status = CaseRefreshStatus.DocumentsRetrieved, FailedReason = null });

            await ScheduleDocumentTasks(context, payload, telemetryEvent);

            await context.CallActivityAsync(nameof(SetCaseStatus), new SetCaseStatusPayload { CaseId = payload.CaseId, UpdatedAt = context.CurrentUtcDateTime, Status = CaseRefreshStatus.Completed, FailedReason = null });

            telemetryEvent.EndTime = context.CurrentUtcDateTime;
        }

        private async Task<GetCaseDocumentsResponse> GetDocuments(TaskOrchestrationContext context, CasePayload payload)
        {
            var documents = await context.CallActivityAsync<GetCaseDocumentsResponse>(nameof(GetCaseDocuments), payload);
            if (!_cmsDocumentsResponseValidator.Validate(documents.CmsDocuments))
            {
                throw new CaseOrchestrationException("Invalid cms documents response: duplicate document ids detected.");
            }

            return documents;
        }

        private async Task<bool> ScheduleDocumentTasks(
            TaskOrchestrationContext context,
            CasePayload casePayload,
            RefreshedCaseEvent telemetryEvent)
        {
            var now = context.CurrentUtcDateTime;
            var deltas = await context.CallActivityAsync<CaseDeltasEntity>(nameof(GetCaseDocumentChanges), casePayload.CaseId);

            var createdOrUpdatedDocuments = deltas.CreatedCmsDocuments.Concat(deltas.UpdatedCmsDocuments).ToList();
            var createdOrUpdatedPcdRequests = deltas.CreatedPcdRequests.Concat(deltas.UpdatedPcdRequests).ToList();
            var createdOrUpdatedDefendantsAndCharges = deltas.CreatedDefendantsAndCharges ?? deltas.UpdatedDefendantsAndCharges;

            var cmsDocumentPayloads = createdOrUpdatedDocuments
                    .Select(documentDelta => new DocumentPayload(
                            casePayload.Urn,
                            casePayload.CaseId,
                            documentDelta.Document.DocumentId,
                            documentDelta.Document.VersionId,
                            documentDelta.Document.Path,
                            documentDelta.Document.CmsDocType,
                            DocumentNature.Types.Document,
                            documentDelta.DeltaType,
                            casePayload.CmsAuthValues,
                            casePayload.CorrelationId,
                            documentDelta.Document.IsOcrProcessed))
                    .ToList();

            var pcdRequestsPayloads
                = createdOrUpdatedPcdRequests
                    .Select(pcd => new DocumentPayload(
                            casePayload.Urn,
                            casePayload.CaseId,
                            pcd.DocumentId,
                            pcd.VersionId,
                            null,
                            pcd.CmsDocType,
                            DocumentNature.Types.PreChargeDecisionRequest,
                            DocumentDeltaType.RequiresIndexing,
                            casePayload.CmsAuthValues,
                            casePayload.CorrelationId))
                    .ToList();

            var defendantsAndChargesPayloads = new List<DocumentPayload>();
            if (createdOrUpdatedDefendantsAndCharges != null)
            {
                defendantsAndChargesPayloads.Add(
                    new DocumentPayload(
                    casePayload.Urn,
                    casePayload.CaseId,
                    createdOrUpdatedDefendantsAndCharges.DocumentId,
                    createdOrUpdatedDefendantsAndCharges.VersionId,
                    null,
                    createdOrUpdatedDefendantsAndCharges.CmsDocType,
                    DocumentNature.Types.DefendantsAndCharges,
                    DocumentDeltaType.RequiresIndexing,
                    casePayload.CmsAuthValues,
                    casePayload.CorrelationId));
            }

            var allPayloads = cmsDocumentPayloads.Concat(pcdRequestsPayloads).Concat(defendantsAndChargesPayloads);

            telemetryEvent.CmsDocsProcessedCount = createdOrUpdatedDocuments.Count;
            telemetryEvent.PcdRequestsProcessedCount = createdOrUpdatedPcdRequests.Count;

            var allTasks = allPayloads.Select(payload => context.CallSubOrchestratorAsync<RefreshDocumentOrchestratorResponse>(
                nameof(RefreshDocumentOrchestrator),
                payload,
                PollingHelper.DefaultActivityOptions)).ToList();

            await Task.WhenAll(allTasks);

            return await context.CallActivityAsync<bool>(
                    nameof(SetDocumentRefreshStatuses),
                    new SetDocumentRefreshStatusesPayload { CaseId = casePayload.CaseId, DocumentRefreshStatuses = allTasks.Select(t => t.Result).ToList() },
                    PollingHelper.DefaultActivityOptions);
        }

        private static async Task<T> BufferCall<T>(Task<T> task)
        {
            try
            {
                return await task;
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}