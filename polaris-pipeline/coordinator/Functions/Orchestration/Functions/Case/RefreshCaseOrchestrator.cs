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
using coordinator.Domain;
using coordinator.Domain.Exceptions;
using coordinator.Domain.Tracker;
using coordinator.Functions.ActivityFunctions.Case;
using coordinator.Functions.Orchestration.Functions.Document;
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
        private readonly TimeSpan _timeout;

        const string loggingName = $"{nameof(RefreshCaseOrchestrator)} - {nameof(Run)}";

        public RefreshCaseOrchestrator(ILogger<RefreshCaseOrchestrator> log, IConfiguration configuration)
        {
            _log = log;
            _configuration = configuration;
            _timeout = TimeSpan.FromSeconds(double.Parse(_configuration[ConfigKeys.CoordinatorKeys.CoordinatorOrchestratorTimeoutSecs]));
        }

        [FunctionName(nameof(RefreshCaseOrchestrator))]
        public async Task<TrackerDto> Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var payload = context.GetInput<CaseOrchestrationPayload>();
            if (payload == null)
                throw new ArgumentException("Orchestration payload cannot be null.", nameof(context));

            var log = context.CreateReplaySafeLogger(_log);

            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Retrieve tracker for case {payload.CmsCaseId}");
            var tracker = CreateOrGetTracker(context, payload.CmsCaseId, payload.CorrelationId, log);

            try
            {
                log.LogMethodFlow(payload.CorrelationId, loggingName, $"Run main orchestration for case {payload.CmsCaseId}");
                var orchestratorTask = RunCaseOrchestrator(context, tracker, payload);

                using var cts = new CancellationTokenSource();
                var deadline = context.CurrentUtcDateTime.Add(_timeout);
                var timeoutTask = context.CreateTimer(deadline, cts.Token);

                var result = await Task.WhenAny(orchestratorTask, timeoutTask);
                if (result == orchestratorTask)
                {
                    // success case
                    cts.Cancel();
                    return await orchestratorTask;
                }

                throw new TimeoutException($"Orchestration with id '{context.InstanceId}' timed out.");
            }
            catch (Exception exception)
            {
                await tracker.RegisterCompleted((context.CurrentUtcDateTime, false));
                log.LogMethodError(payload.CorrelationId, loggingName, $"Error when running {nameof(RefreshCaseOrchestrator)} orchestration with id '{context.InstanceId}'", exception);
                throw;
            }
            finally
            {
                log.LogMethodExit(payload.CorrelationId, loggingName, string.Empty);
            }
        }

        private async Task<TrackerDto> RunCaseOrchestrator(IDurableOrchestrationContext context, ITrackerEntity tracker, CaseOrchestrationPayload payload)
        {
            const string loggingName = nameof(RunCaseOrchestrator);
            var log = context.CreateReplaySafeLogger(_log);

            log.LogMethodEntry(payload.CorrelationId, loggingName, payload.ToJson());
            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Resetting tracker for {context.InstanceId}");
            await tracker.Reset((context.CurrentUtcDateTime, context.InstanceId));

            var documents = await GetDocuments(context, tracker, loggingName, log, payload);

            var documentTasks = await GetDocumentTasks(context, tracker, 1, payload, documents, log);
            for(var retry=1; retry <= 3 && documentTasks.Any(); retry++)
            {
                await Task.WhenAll(documentTasks.Select(BufferCall));

                if (await tracker.AnyDocumentsFailed())
                {
                    documentTasks = await GetDocumentTasks(context, tracker, retry+1, payload, documents, log);
                }
            }

            if (await tracker.AllDocumentsFailed())
                throw new CaseOrchestrationException("Cms Documents, PCD Requests or Defendants and Charges failed to process during orchestration.");

            await tracker.RegisterCompleted((context.CurrentUtcDateTime, true));

            log.LogMethodExit(payload.CorrelationId, loggingName, "Returning tracker");

            var trackerDto = tracker.Adapt<TrackerDto>();
            return trackerDto;
        }

        private async static Task<List<Task>> GetDocumentTasks
            (
                IDurableOrchestrationContext context,
                ITrackerEntity tracker,
                int retry,
                CaseOrchestrationPayload caseDocumentPayload,
                (DocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges) documents,
                ILogger log
            )
        {
            var deltas = await GetCaseDocumentChanges(context.CurrentUtcDateTime, tracker, loggingName, log, caseDocumentPayload, documents);

            var logMessage = $"Refresh Documents, retry {retry}, CMS:({deltas.CreatedCmsDocuments.Count} created, {deltas.UpdatedCmsDocuments.Count} updated, {deltas.DeletedCmsDocuments.Count} deleted). PCD :({deltas.CreatedPcdRequests.Count} created, {deltas.DeletedPcdRequests.Count} deleted)";
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
            var allTasks = allPayloads.Select(payload => context.CallSubOrchestratorAsync(nameof(RefreshDocumentOrchestrator), payload));

            return allTasks.ToList();
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

        private async Task<(DocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges)> GetDocuments(IDurableOrchestrationContext context, ITrackerEntity tracker, string nameToLog, ILogger safeLogger, CaseOrchestrationPayload payload)
        {
            safeLogger.LogMethodFlow(payload.CorrelationId, nameToLog, $"Getting Documents for case {payload.CmsCaseId}");

            var getCaseEntitiesActivityPayload = new GetCaseDocumentsActivityPayload(payload.CmsCaseUrn, payload.CmsCaseId, payload.CmsAuthValues, payload.CorrelationId);
            var documents = await context.CallActivityAsync<(DocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges)>(nameof(GetCaseDocuments), getCaseEntitiesActivityPayload);

            return documents;
        }

        private static async Task<TrackerDeltasDto> GetCaseDocumentChanges
            (
                DateTime t,
                ITrackerEntity tracker, 
                string nameToLog, 
                ILogger safeLogger, 
                BasePipelinePayload payload,
                (DocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges) documents
            )
        {
            safeLogger.LogMethodFlow(payload.CorrelationId, nameToLog, $"Documents found, register document Ids in tracker for case {payload.CmsCaseId}");

            var synchroniseDocumentArgs = (t, payload.CmsCaseUrn, payload.CmsCaseId, documents.CmsDocuments, documents.PcdRequests, documents.DefendantsAndCharges, payload.CorrelationId);
            var deltas = await tracker.GetCaseDocumentChanges(synchroniseDocumentArgs);

            return deltas;
        }
    }
}