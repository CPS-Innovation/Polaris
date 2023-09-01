#if SCALABILITY_TEST
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Dto.Document;
using Common.Dto.Tracker;
using Common.Telemetry.Contracts;
using coordinator.Domain;
using coordinator.Functions.ActivityFunctions.Case;
using coordinator.Functions.DurableEntity.Entity;
using coordinator.Functions.DurableEntity.Entity.Contract;
using coordinator.Functions.Orchestration.Functions.Document;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;

namespace coordinator.Functions.Orchestration.Functions.Case
{
    public class ScalabilityTestCaseOrchestrator
    {
        private readonly IConfiguration _configuration;
        private readonly ITelemetryClient _telemetryClient;
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(600);

        const string loggingName = $"{nameof(ScalabilityTestCaseOrchestrator)} - {nameof(Run)}";

        public static string GetKey(long caseId)
        {
            return $"[ScalabilityTest-{caseId}]";
        }

        public ScalabilityTestCaseOrchestrator(IConfiguration configuration, ITelemetryClient telemetryClient)
        {
            _configuration = configuration;
            _telemetryClient = telemetryClient;
        }

        [FunctionName(nameof(ScalabilityTestCaseOrchestrator))]
        public async Task<bool> Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var payload = context.GetInput<ScalabilityTestCaseOrchestrationPayload>();

            var scalablityTestEntity = await CreateOrGetCaseDurableEntities(context, payload.CaseId);
            scalablityTestEntity.SetCaseStatus((context.CurrentUtcDateTime, CaseRefreshStatus.Running, null));

            try
            {
                var orchestratorTask = RunScalabilityTestOrchestrator(context, scalablityTestEntity, payload);

                var result = await Task.WhenAny(orchestratorTask);
                return await orchestratorTask;
            }
            catch (Exception exception)
            {
                scalablityTestEntity.SetCaseStatus((context.CurrentUtcDateTime, CaseRefreshStatus.Failed, exception.Message));
            }

            return false;
        }

        private async Task<bool> RunScalabilityTestOrchestrator(IDurableOrchestrationContext context, IScalabilityTestDurableEntity scalabilityTestEntity, ScalabilityTestCaseOrchestrationPayload payload)
        {
            scalabilityTestEntity.Reset(context.InstanceId);
            scalabilityTestEntity.SetCaseStatus((context.CurrentUtcDateTime, CaseRefreshStatus.Running, null));

            var documents = await GetDocuments(context, payload.DocumentCount);
            scalabilityTestEntity.SetCaseStatus((context.CurrentUtcDateTime, CaseRefreshStatus.DocumentsRetrieved, null));

            var documentTasks = await GetDocumentTasks(context, scalabilityTestEntity, payload, documents);
            await Task.WhenAll(documentTasks);

            scalabilityTestEntity.SetCaseStatus((context.CurrentUtcDateTime, CaseRefreshStatus.Completed, null));

            return await Task.FromResult(true);
        }

        protected async Task<IScalabilityTestDurableEntity> CreateOrGetCaseDurableEntities(IDurableOrchestrationContext context, long caseId)
        {
            var caseEntityKey = ScalabilityTestDurableEntity.GetOrchestrationKey(caseId);
            var caseEntityId = new EntityId(nameof(ScalabilityTestDurableEntity), caseEntityKey);
            var caseEntity = context.CreateEntityProxy<IScalabilityTestDurableEntity>(caseEntityId);

            return await Task.FromResult(caseEntity);
        }

        private async Task<CmsDocumentDto[]> GetDocuments(IDurableOrchestrationContext context, int documentCount)
        {
            var getScalabilityTestDocumentsActivityPayload = new GetScalabilityTestDocumentsActivityPayload(documentCount);
            var documents = await context.CallActivityAsync<CmsDocumentDto[]>(nameof(GetScalabilityTestDocuments), getScalabilityTestDocumentsActivityPayload);

            return documents;
        }

        private async static Task<List<Task>> GetDocumentTasks
            (
                IDurableOrchestrationContext context,
                IScalabilityTestDurableEntity scalabilityTestDurableEntity,
                ScalabilityTestCaseOrchestrationPayload caseDocumentPayload,
                CmsDocumentDto[] documents
            )
        {
            var now = context.CurrentUtcDateTime;

            var deltas = await scalabilityTestDurableEntity.GetCaseDocumentChanges(documents);

            var payloads
                = deltas
                    .Select
                    (
                        documentId =>
                        {
                            return (caseDocumentPayload.CaseId, documentId);
                        }
                    )
                    .ToList();

            var allTasks = payloads.Select
                    (
                        payload => context.CallSubOrchestratorAsync
                        (
                            nameof(ScalabilityTestDocumentOrchestrator),
                            ScalabilityTestDocumentOrchestrator.GetKey(payload.CaseId, payload.documentId),
                            payload)
                        );

            return allTasks.ToList();
        }
    }
}
#endif