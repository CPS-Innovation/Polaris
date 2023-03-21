using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Constants;
using Common.Domain.DocumentEvaluation;
using Common.Domain.DocumentExtraction;
using Common.Domain.Extensions;
using Common.Logging;
using coordinator.Domain;
using coordinator.Domain.Exceptions;
using coordinator.Domain.Tracker;
using coordinator.Functions.ActivityFunctions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.OrchestrationFunctions
{
    public class RefreshCaseOrchestrator
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
        public async Task<List<TrackerDocument>> Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var payload = context.GetInput<CaseOrchestrationPayload>();
            if (payload == null)
                throw new ArgumentException("Orchestration payload cannot be null.", nameof(context));

            var log = context.CreateReplaySafeLogger(_log);
            var currentCaseId = payload.CmsCaseId;

            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Retrieve tracker for case {currentCaseId}");
            var tracker = CreateTracker(context, payload.CmsCaseUrn, payload.CmsCaseId, payload.CorrelationId, log);

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

                // timeout case
                throw new TimeoutException($"Orchestration with id '{context.InstanceId}' timed out.");
            }
            catch (Exception exception)
            {
                log.LogMethodFlow(payload.CorrelationId, loggingName, "Registering Failure in the tracker");
                await tracker.RegisterFailed();
                log.LogMethodError(payload.CorrelationId, loggingName, $"Error when running {nameof(RefreshCaseOrchestrator)} orchestration with id '{context.InstanceId}'", exception);
                throw;
            }
            finally
            {
                log.LogMethodExit(payload.CorrelationId, loggingName, string.Empty);
            }
        }

        private async Task<List<TrackerDocument>> RunCaseOrchestrator(IDurableOrchestrationContext context, ITracker tracker, CaseOrchestrationPayload payload)
        {
            const string loggingName = nameof(RunCaseOrchestrator);
            var log = context.CreateReplaySafeLogger(_log);

            log.LogMethodEntry(payload.CorrelationId, loggingName, payload.ToJson());

            if (await tracker.IsAlreadyProcessed() && !await tracker.IsStale(payload.ForceRefresh))
            {
                log.LogMethodFlow(payload.CorrelationId, loggingName,
                    $"Tracker has already finished processing, a 'force refresh' has not been issued and it is not stale - returning documents - {context.InstanceId}");
                return await tracker.GetDocuments();
            }

            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Initialising tracker for {context.InstanceId}");
            await tracker.Initialise(context.InstanceId);

            var documents = await RetrieveDocuments(context, tracker, loggingName, log, payload);
            if (documents.Length == 0)
                return new List<TrackerDocument>();

            //bring the tracker document list up-to-date, or populate it for the first time
            await RegisterDocuments(context, tracker, loggingName, log, payload, documents);
            var trackerDocuments = await tracker.GetDocuments();

            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Now process each document for case {payload.CmsCaseId}");
            var caseDocumentTasks
                = trackerDocuments.Select
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

            await Task.WhenAll(caseDocumentTasks.Select(BufferCall));

            if (await tracker.AllDocumentsFailed())
                throw new CaseOrchestrationException("All documents failed to process during orchestration.");

            log.LogMethodFlow(payload.CorrelationId, loggingName, $"All documents processed successfully for case {payload.CmsCaseId}");
            await tracker.RegisterCompleted();

            log.LogMethodExit(payload.CorrelationId, loggingName, "Returning documents");
            trackerDocuments = await tracker.GetDocuments();
            return trackerDocuments;
        }

        private static async Task BufferCall(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception)
            {
                // ReSharper disable once RedundantJumpStatement
                return;
            }
        }

        private ITracker CreateTracker(IDurableOrchestrationContext context, string caseUrn, long caseId, Guid correlationId, ILogger safeLoggerInstance)
        {
            safeLoggerInstance.LogMethodEntry(correlationId, nameof(CreateTracker), $"CaseUrn: {caseUrn}, CaseId: {caseId}");

            var entityId = new EntityId(nameof(Tracker), caseId.ToString());
            var result = context.CreateEntityProxy<ITracker>(entityId);

            safeLoggerInstance.LogMethodExit(correlationId, nameof(CreateTracker), "n/a");
            return result;
        }

        private async Task<CmsCaseDocument[]> RetrieveDocuments(IDurableOrchestrationContext context, ITracker tracker, string nameToLog, ILogger safeLogger,
            CaseOrchestrationPayload payload)
        {
            safeLogger.LogMethodFlow(payload.CorrelationId, nameToLog, $"Getting list of documents for case {payload.CmsCaseId}");
            var documents = await context.CallActivityAsync<CmsCaseDocument[]>(
                nameof(GetCaseDocuments),
                new GetCaseDocumentsActivityPayload(payload.CmsCaseUrn, payload.CmsCaseId, payload.CmsAuthValues, payload.CorrelationId));

            return documents;
        }

        private static async Task RegisterDocuments(IDurableOrchestrationContext context, ITracker tracker, string nameToLog, ILogger safeLogger, BasePipelinePayload payload,
            IEnumerable<CmsCaseDocument> documents)
        {
            safeLogger.LogMethodFlow(payload.CorrelationId, nameToLog, $"Documents found, register document Ids in tracker for case {payload.CmsCaseId}");
            List<IncomingDocument> incomingDocuments
                = documents
                    .Select(item => new IncomingDocument(polarisDocumentId: context.NewGuid(),
                                                         documentId: item.DocumentId,
                                                         versionId: item.VersionId,
                                                         originalFileName: item.FileName,
                                                         mimeType: item.MimeType,
                                                         cmsDocType: item.CmsDocType,
                                                         createdDate: item.DocumentDate
                                                         )
                           )
                    .ToList();
            var arg = new RegisterDocumentIdsArg(payload.CmsCaseUrn, payload.CmsCaseId, incomingDocuments, payload.CorrelationId);
            await tracker.RegisterDocumentIds(arg);
        }
    }
}