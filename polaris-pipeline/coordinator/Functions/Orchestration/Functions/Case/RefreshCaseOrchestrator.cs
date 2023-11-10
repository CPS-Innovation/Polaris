using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Common.Constants;
using Common.Domain.Extensions;
using Common.Dto.Case;
using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using Common.Dto.Tracker;
using Common.Logging;
using Common.Telemetry.Contracts;
using coordinator.Domain;
using coordinator.Domain.Exceptions;
using coordinator.Functions.ActivityFunctions.Case;
using coordinator.Functions.DurableEntity.Entity.Contract;
using coordinator.Functions.Orchestration.Functions.Document;
using coordinator.TelemetryEvents;
using Mapster;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.Orchestration.Functions.Case
{
    public class RefreshCaseOrchestrator : PolarisOrchestrator
    {
        private readonly ILogger<RefreshCaseOrchestrator> _log;
        private readonly IConfiguration _configuration;
        private readonly ITelemetryClient _telemetryClient;
        private readonly TimeSpan _timeout;

        const string loggingName = $"{nameof(RefreshCaseOrchestrator)} - {nameof(Run)}";

        public static string GetKey(string caseId)
        {
            return $"[{caseId}]";
        }

        public RefreshCaseOrchestrator(ILogger<RefreshCaseOrchestrator> log, IConfiguration configuration, ITelemetryClient telemetryClient)
        {
            _log = log;
            _configuration = configuration;
            _telemetryClient = telemetryClient;
            _timeout = TimeSpan.FromSeconds(double.Parse(_configuration[ConfigKeys.CoordinatorKeys.CoordinatorOrchestratorTimeoutSecs]));
        }

        [FunctionName(nameof(RefreshCaseOrchestrator))]
        public async Task<TrackerDto> Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var payload = context.GetInput<CaseOrchestrationPayload>();
            if (payload == null)
                throw new ArgumentException("Orchestration payload cannot be null.", nameof(context));

            var log = context.CreateReplaySafeLogger(_log);
            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Retrieve case trackers for case {payload.CmsCaseId}");
            var caseEntity = await CreateOrGetCaseDurableEntity(context, payload.CmsCaseId, true, payload.CorrelationId, log);
            caseEntity.SetCaseStatus((context.CurrentUtcDateTime, CaseRefreshStatus.Running, null));

            RefreshedCaseEvent telemetryEvent = default;
            try
            {
                telemetryEvent = new RefreshedCaseEvent(
                    correlationId: payload.CorrelationId,
                    caseId: payload.CmsCaseId,
                    versionId: await caseEntity.GetVersion(),
                    startTime: await caseEntity.GetStartTime()
                );

                log.LogMethodFlow(payload.CorrelationId, loggingName, $"Run main orchestration for case {payload.CmsCaseId}");
                var orchestratorTask = RunCaseOrchestrator(context, caseEntity, payload, telemetryEvent);

                using var cts = new CancellationTokenSource();
                var deadline = context.CurrentUtcDateTime.Add(_timeout);
                var timeoutTask = context.CreateTimer(deadline, cts.Token);

                var result = await Task.WhenAny(orchestratorTask, timeoutTask);
                if (result == orchestratorTask)
                {
                    // success case
                    cts.Cancel();
                    _telemetryClient.TrackEvent(telemetryEvent);
                    return await orchestratorTask;
                }

                throw new TimeoutException($"Orchestration with id '{context.InstanceId}' timed out.");
            }
            catch (Exception exception)
            {
                caseEntity.SetCaseStatus((context.CurrentUtcDateTime, CaseRefreshStatus.Failed, exception.Message));

                log.LogMethodError(payload.CorrelationId, loggingName, $"Error when running {nameof(RefreshCaseOrchestrator)} orchestration with id '{context.InstanceId}'", exception);
                _telemetryClient.TrackEventFailure(telemetryEvent);
                throw;
            }
            finally
            {
                log.LogMethodExit(payload.CorrelationId, loggingName, string.Empty);
            }
        }

        private async Task<TrackerDto> RunCaseOrchestrator(IDurableOrchestrationContext context, ICaseDurableEntity caseEntity, CaseOrchestrationPayload payload, RefreshedCaseEvent telemetryEvent)
        {
            const string loggingName = nameof(RunCaseOrchestrator);

            var log = context.CreateReplaySafeLogger(_log);

            log.LogMethodEntry(payload.CorrelationId, loggingName, payload.ToJson());
            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Resetting case entity for {context.InstanceId}");
            caseEntity.Reset(context.InstanceId);
            caseEntity.SetCaseStatus((context.CurrentUtcDateTime, CaseRefreshStatus.Running, null));

            var documents = await GetDocuments(context, loggingName, log, payload);
            telemetryEvent.CmsDocsCount = documents.CmsDocuments.Length;
            caseEntity.SetCaseStatus((context.CurrentUtcDateTime, CaseRefreshStatus.DocumentsRetrieved, null));

            var (documentTasks, cmsDocsProcessedCount, pcdRequestsProcessedCount) = await GetDocumentTasks(context, caseEntity, payload, documents, log);
            telemetryEvent.CmsDocsProcessedCount = cmsDocsProcessedCount;
            telemetryEvent.PcdRequestsProcessedCount = pcdRequestsProcessedCount;
            await Task.WhenAll(documentTasks.Select(BufferCall));

            if (await caseEntity.AllDocumentsFailed())
                throw new CaseOrchestrationException("CMS Documents, PCD Requests or Defendants and Charges failed to process during orchestration.");

            caseEntity.SetCaseStatus((context.CurrentUtcDateTime, CaseRefreshStatus.Completed, null));

            log.LogMethodExit(payload.CorrelationId, loggingName, "Returning tracker");
            telemetryEvent.EndTime = context.CurrentUtcDateTime;

            return caseEntity.Adapt<TrackerDto>();
        }

        private async static Task<(List<Task>, int, int)> GetDocumentTasks
            (
                IDurableOrchestrationContext context,
                ICaseDurableEntity caseTracker,
                CaseOrchestrationPayload caseDocumentPayload,
                (CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges) documents,
                ILogger log
            )
        {
            var now = context.CurrentUtcDateTime;

            var deltas = await caseTracker.GetCaseDocumentChanges((documents.CmsDocuments, documents.PcdRequests, documents.DefendantsAndCharges));
            var logMessage = deltas.GetLogMessage();
            log.LogMethodFlow(caseDocumentPayload.CorrelationId, loggingName, logMessage);

            var createdOrUpdatedDocuments = deltas.CreatedCmsDocuments.Concat(deltas.UpdatedCmsDocuments).ToList();
            var createdOrUpdatedPcdRequests = deltas.CreatedPcdRequests.Concat(deltas.UpdatedPcdRequests).ToList();
            var createdOrUpdatedDefendantsAndCharges = deltas.CreatedDefendantsAndCharges ?? deltas.UpdatedDefendantsAndCharges;

            var cmsDocumentPayloads
                = createdOrUpdatedDocuments
                    .Select
                    (
                        trackerCmsDocument =>
                        {
                            return new CaseDocumentOrchestrationPayload
                            (
                                caseDocumentPayload.CmsAuthValues,
                                caseDocumentPayload.CorrelationId,
                                caseDocumentPayload.CmsCaseUrn,
                                caseDocumentPayload.CmsCaseId,
                                JsonSerializer.Serialize(trackerCmsDocument),
                                null,
                                null
                            );
                        }
                    )
                    .ToList();

            var pcdRequestsPayloads
                = createdOrUpdatedPcdRequests
                    .Select
                    (
                        trackerPcdRequest =>
                        {
                            return new CaseDocumentOrchestrationPayload
                            (
                                caseDocumentPayload.CmsAuthValues,
                                caseDocumentPayload.CorrelationId,
                                caseDocumentPayload.CmsCaseUrn,
                                caseDocumentPayload.CmsCaseId,
                                null,
                                JsonSerializer.Serialize(trackerPcdRequest),
                                null
                            );
                        }
                    ).
                    ToList();

            var defendantsAndChargesPayloads = new List<CaseDocumentOrchestrationPayload>();
            if (createdOrUpdatedDefendantsAndCharges != null)
            {
                var payload = new CaseDocumentOrchestrationPayload
                (
                    caseDocumentPayload.CmsAuthValues,
                    caseDocumentPayload.CorrelationId,
                    caseDocumentPayload.CmsCaseUrn,
                    caseDocumentPayload.CmsCaseId,
                    null,
                    null,
                    JsonSerializer.Serialize(createdOrUpdatedDefendantsAndCharges)
                );
                defendantsAndChargesPayloads.Add(payload);
            }

            var allPayloads = cmsDocumentPayloads.Concat(pcdRequestsPayloads).Concat(defendantsAndChargesPayloads);
            var allTasks = allPayloads.Select
                    (
                        payload => context.CallSubOrchestratorAsync
                        (
                            nameof(RefreshDocumentOrchestrator),
                            RefreshDocumentOrchestrator.GetKey(payload.CmsCaseId, payload.PolarisDocumentId),
                            payload)
                        );

            return (allTasks.ToList(), createdOrUpdatedDocuments.Count, createdOrUpdatedPcdRequests.Count);
        }

        private static async Task BufferCall(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception)
            {
                return;
            }
        }

        private async Task<(CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges)>
            GetDocuments(IDurableOrchestrationContext context, string nameToLog, ILogger logger, CaseOrchestrationPayload payload)
        {
            logger.LogMethodFlow(payload.CorrelationId, nameToLog, $"Getting Documents for case {payload.CmsCaseId}");

            var getCaseEntitiesActivityPayload = new GetCaseDocumentsActivityPayload(payload.CmsCaseUrn, payload.CmsCaseId, payload.CmsAuthValues, payload.CorrelationId);
            var documents = await context.CallActivityAsync<(CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges)>(nameof(GetCaseDocuments), getCaseEntitiesActivityPayload);

            return documents;
        }
    }
}