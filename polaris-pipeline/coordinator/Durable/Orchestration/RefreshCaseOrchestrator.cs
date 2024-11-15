using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Common.Dto.Response;
using Common.Dto.Response.Documents;
using Common.Logging;
using Common.Telemetry;
using coordinator.Constants;
using coordinator.Domain.Exceptions;
using coordinator.Durable.Activity;
using coordinator.Durable.Entity;
using coordinator.Durable.Payloads;
using coordinator.Durable.Payloads.Domain;
using coordinator.TelemetryEvents;
using coordinator.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Common.Domain.Document;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

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

            var caseEntity = CaseDurableEntity.GetEntityId(payload.CaseId);

            caseEntity.SetCaseStatus((context.CurrentUtcDateTime, CaseRefreshStatus.Running, null));

            RefreshedCaseEvent telemetryEvent = default;
            try
            {
                telemetryEvent = new RefreshedCaseEvent(
                    correlationId: payload.CorrelationId,
                    caseId: payload.CaseId,
                    startTime: await caseEntity.GetStartTime()
                );

                var orchestratorTask = RunCaseOrchestrator(context, caseEntity, payload, telemetryEvent, log);

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
                caseEntity.SetCaseStatus((context.CurrentUtcDateTime, CaseRefreshStatus.Failed, exception.Message));

                log.LogMethodError(payload.CorrelationId, nameof(RefreshCaseOrchestrator), $"Error when running {nameof(RefreshCaseOrchestrator)} orchestration with id '{context.InstanceId}'", exception);
                _telemetryClient.TrackEventFailure(telemetryEvent);
                throw;
            }
        }

        private async Task RunCaseOrchestrator(TaskOrchestrationContext context, ICaseDurableEntity caseEntity, CasePayload payload, RefreshedCaseEvent telemetryEvent, ILogger logger)
        {
            caseEntity.Reset();
            caseEntity.SetCaseStatus((context.CurrentUtcDateTime, CaseRefreshStatus.Running, null));

            var documents = await GetDocuments(context, payload);
            telemetryEvent.CmsDocsCount = documents.CmsDocuments.Length;
            caseEntity.SetCaseStatus((context.CurrentUtcDateTime, CaseRefreshStatus.DocumentsRetrieved, null));

            var (documentTasks, cmsDocsProcessedCount, pcdRequestsProcessedCount) = await GetDocumentTasks(context, caseEntity, payload, documents, logger);
            telemetryEvent.CmsDocsProcessedCount = cmsDocsProcessedCount;
            telemetryEvent.PcdRequestsProcessedCount = pcdRequestsProcessedCount;
            await Task.WhenAll(documentTasks.Select(BufferCall));

            caseEntity.SetCaseStatus((context.CurrentUtcDateTime, CaseRefreshStatus.Completed, null));

            telemetryEvent.EndTime = context.CurrentUtcDateTime;
        }

        private async Task<(CmsDocumentDto[] CmsDocuments, PcdRequestCoreDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges)>
        GetDocuments(TaskOrchestrationContext context, CasePayload payload)
        {
            var documents = await context.CallActivityAsync<(CmsDocumentDto[] CmsDocuments, PcdRequestCoreDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges)>(nameof(GetCaseDocuments), payload);
            if (!_cmsDocumentsResponseValidator.Validate(documents.CmsDocuments))
            {
                throw new CaseOrchestrationException("Invalid cms documents response: duplicate document ids detected.");
            }
            return documents;
        }

        private async Task<(List<Task<RefreshDocumentResult>>, int, int)> GetDocumentTasks
            (
                TaskOrchestrationContext context,
                ICaseDurableEntity caseTracker,
                CasePayload casePayload,
                (CmsDocumentDto[] CmsDocuments, PcdRequestCoreDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges) documents,
                ILogger log
            )
        {
            var now = context.CurrentUtcDateTime;
            var deltas = await caseTracker.GetCaseDocumentChanges((documents.CmsDocuments, documents.PcdRequests, documents.DefendantsAndCharges));

            var createdOrUpdatedDocuments = deltas.CreatedCmsDocuments.Concat(deltas.UpdatedCmsDocuments).ToList();
            var createdOrUpdatedPcdRequests = deltas.CreatedPcdRequests.Concat(deltas.UpdatedPcdRequests).ToList();
            var createdOrUpdatedDefendantsAndCharges = deltas.CreatedDefendantsAndCharges ?? deltas.UpdatedDefendantsAndCharges;

            var cmsDocumentPayloads
                = createdOrUpdatedDocuments
                    .Select(((CmsDocumentEntity doc, DocumentDeltaType delta) item) => new DocumentPayload(
                            casePayload.Urn,
                            casePayload.CaseId,
                            item.doc.DocumentId,
                            item.doc.VersionId,
                            item.doc.Path,
                            item.doc.CmsDocType,
                            DocumentNature.Types.Document,
                            item.delta,
                            casePayload.CmsAuthValues,
                            casePayload.CorrelationId,
                            item.doc.IsOcrProcessed)
                    ).ToList();

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
                            casePayload.CorrelationId)
                    ).ToList();

            var defendantsAndChargesPayloads = new List<DocumentPayload>();
            if (createdOrUpdatedDefendantsAndCharges != null)
            {
                defendantsAndChargesPayloads.Add(new DocumentPayload
                (
                    casePayload.Urn,
                    casePayload.CaseId,
                    createdOrUpdatedDefendantsAndCharges.DocumentId,
                    createdOrUpdatedDefendantsAndCharges.VersionId,
                    null,
                    createdOrUpdatedDefendantsAndCharges.CmsDocType,
                    DocumentNature.Types.DefendantsAndCharges,
                    DocumentDeltaType.RequiresIndexing,
                    casePayload.CmsAuthValues,
                    casePayload.CorrelationId
                ));
            }

            var allPayloads = cmsDocumentPayloads.Concat(pcdRequestsPayloads).Concat(defendantsAndChargesPayloads);

            var allTasks = allPayloads.Select
                    (
                        payload => context.CallSubOrchestratorAsync<RefreshDocumentResult>
                        (
                            nameof(RefreshDocumentOrchestrator),
                            payload,
                            PollingHelper.DefaultActivityOptions
                        )
                    );

            return (allTasks.ToList(), createdOrUpdatedDocuments.Count, createdOrUpdatedPcdRequests.Count);
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