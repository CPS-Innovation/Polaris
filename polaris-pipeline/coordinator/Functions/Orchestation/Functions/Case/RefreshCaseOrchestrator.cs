using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Constants;
using Common.Domain.Extensions;
using Common.Dto.Tracker;
using Common.Logging;
using coordinator.Domain;
using coordinator.Domain.Exceptions;
using coordinator.Domain.Tracker;
using coordinator.Functions.ActivityFunctions.Case;
using coordinator.Functions.Orchestation.Functions.Document;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.Orchestation.Functions.Case
{
    public class RefreshCaseOrchestrator : PolarisOrchestrator
    {
        private readonly ILogger<RefreshCaseOrchestrator> _log;
        private readonly IConfiguration _configuration;

        const string loggingName = $"{nameof(RefreshCaseOrchestrator)} - {nameof(Run)}";

        public RefreshCaseOrchestrator(ILogger<RefreshCaseOrchestrator> log, IConfiguration configuration)
        {
            _log = log;
            _configuration = configuration;
        }

        [FunctionName(nameof(RefreshCaseOrchestrator))]
        public async Task<TrackerDocumentListDeltasDto> Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var payload = context.GetInput<CaseOrchestrationPayload>();
            if (payload == null)
                throw new ArgumentException("Orchestration payload cannot be null.", nameof(context));

            var log = context.CreateReplaySafeLogger(_log);
            var currentCaseId = payload.CmsCaseId;

            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Retrieve tracker for case {currentCaseId}");
            var tracker = CreateOrGetTracker(context, payload.CmsCaseId, payload.CorrelationId, log);

            try
            {
                var timeout = TimeSpan.FromSeconds(double.Parse(_configuration[ConfigKeys.CoordinatorKeys.CoordinatorOrchestratorTimeoutSecs]));
                var deadline = context.CurrentUtcDateTime.Add(timeout);

                using var cts = new CancellationTokenSource();
                log.LogMethodFlow(payload.CorrelationId, loggingName, $"Run main orchestration for case {currentCaseId}");
                var orchestratorTask = RunCaseOrchestrator(context, tracker, payload);
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
                await tracker.RegisterFailed();
                log.LogMethodError(payload.CorrelationId, loggingName, $"Error when running {nameof(RefreshCaseOrchestrator)} orchestration with id '{context.InstanceId}'", exception);
                throw;
            }
            finally
            {
                log.LogMethodExit(payload.CorrelationId, loggingName, string.Empty);
            }
        }

        private async Task<TrackerDocumentListDeltasDto> RunCaseOrchestrator(IDurableOrchestrationContext context, ITrackerEntity tracker, CaseOrchestrationPayload payload)
        {
            const string loggingName = nameof(RunCaseOrchestrator);
            var log = context.CreateReplaySafeLogger(_log);

            log.LogMethodEntry(payload.CorrelationId, loggingName, payload.ToJson());

            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Resetting tracker for {context.InstanceId}");
            await tracker.Reset(context.InstanceId);

            var transitionDocuments = await RetrieveTransitionDocuments(context, tracker, loggingName, log, payload);
            var deltas = await SynchroniseTrackerDocuments(tracker, loggingName, log, payload, transitionDocuments);

            log.LogMethodFlow(payload.CorrelationId, loggingName, $"{deltas.Created.Count} document created, {deltas.Updated.Count} updated and {deltas.Deleted.Count} document deleted for case {payload.CmsCaseId}");
            var refreshDocuments = deltas.Created.Concat(deltas.Updated).ToList();

            var caseDocumentTasks
                = refreshDocuments
                    .Select
                    (
                        t => context.CallSubOrchestratorAsync
                                (
                                    nameof(RefreshDocumentOrchestrator),
                                    new CaseDocumentOrchestrationPayload
                                    (
                                        t.PolarisDocumentId,
                                        payload.CmsCaseUrn,
                                        payload.CmsCaseId,
                                        t.CmsDocType.DocumentCategory,
                                        t.CmsDocumentId,
                                        t.CmsVersionId,
                                        t.CmsOriginalFileName,
                                        payload.CmsAuthValues,
                                        payload.CorrelationId
                                    )
                                )
                    )
                    .ToList();

            var changed = deltas.Any();

            if (changed)
            {
                await Task.WhenAll(caseDocumentTasks.Select(BufferCall));

                if (await tracker.AllDocumentsFailed())
                    throw new CaseOrchestrationException("All documents failed to process during orchestration.");
            }

            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Documents Refreshed, {deltas.Created.Count} created, {deltas.Updated.Count} updated, {deltas.Deleted.Count} deleted");

            await tracker.RegisterCompleted();

            log.LogMethodExit(payload.CorrelationId, loggingName, "Returning changed documents");
            return deltas;
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

        private async Task<TransitionDocument[]> RetrieveTransitionDocuments(IDurableOrchestrationContext context, ITrackerEntity tracker, string nameToLog, ILogger safeLogger, CaseOrchestrationPayload payload)
        {
            safeLogger.LogMethodFlow(payload.CorrelationId, nameToLog, $"Getting list of transition documents for case {payload.CmsCaseId}");
            var documents = await context.CallActivityAsync<TransitionDocument[]>(
                nameof(GetCaseDocuments),
                new GetCaseDocumentsActivityPayload(payload.CmsCaseUrn, payload.CmsCaseId, payload.CmsAuthValues, payload.CorrelationId));

            return documents;
        }

        private static async Task<TrackerDocumentListDeltasDto> SynchroniseTrackerDocuments(ITrackerEntity tracker, string nameToLog, ILogger safeLogger, BasePipelinePayload payload, TransitionDocument[] documents)
        {
            safeLogger.LogMethodFlow(payload.CorrelationId, nameToLog, $"Documents found, register document Ids in tracker for case {payload.CmsCaseId}");

            var arg = new SynchroniseDocumentsArg(payload.CmsCaseUrn, payload.CmsCaseId, documents, payload.CorrelationId);
            var deltas = await tracker.SynchroniseDocuments(arg);

            return deltas;
        }
    }
}