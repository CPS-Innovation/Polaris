using System;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Constants;
using Common.Logging;
using coordinator.Domain;
using coordinator.Functions.DurableEntity.Entity;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.Orchestration.Functions.Case
{
    public class DeleteCaseOrchestrator : PolarisOrchestrator
    {
        private readonly ILogger<RefreshCaseOrchestrator> _log;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _timeout;

        const string loggingName = $"{nameof(RefreshCaseOrchestrator)} - {nameof(Run)}";

        public DeleteCaseOrchestrator(ILogger<RefreshCaseOrchestrator> log, IConfiguration configuration)
        {
            _log = log;
            _configuration = configuration;
            _timeout = TimeSpan.FromSeconds(double.Parse(_configuration[ConfigKeys.CoordinatorKeys.CoordinatorOrchestratorTimeoutSecs]));
        }

        [FunctionName(nameof(DeleteCaseOrchestrator))]
        public async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            #region Validate-Inputs
            var payload = context.GetInput<CaseOrchestrationPayload>();
            if (payload == null)
                throw new ArgumentException("Orchestration payload cannot be null.", nameof(context));
            #endregion

            var log = context.CreateReplaySafeLogger(_log);

            try
            {
                log.LogMethodFlow(payload.CorrelationId, loggingName, $"Retrieve case entities for case {payload.CmsCaseId}");
                var (caseEntity, caseRefreshLogsEntity) = await CreateOrGetCaseDurableEntities(context, payload.CmsCaseId, true, payload.CorrelationId, log);

                var version = await caseEntity.GetVersion();
                var polarisDocumentIds = await caseEntity.GetPolarisDocumentIds();
                var caseId = payload.CmsCaseId.ToString();

                log.LogMethodFlow(payload.CorrelationId, loggingName, $"Deleting Orchestrations and Durable Entities for case {payload.CmsCaseId}");

                await DeleteInstance(context, payload.BaseUrl, payload.ExtensionCode, caseId);
                foreach (var polarisDocumentId in polarisDocumentIds)
                    await DeleteInstance(context, payload.BaseUrl, payload.ExtensionCode, $"{caseId}-{polarisDocumentId}");

                await DeleteInstance(context, payload.BaseUrl, payload.ExtensionCode, $"@{nameof(CaseDurableEntity).ToLower()}@{caseId}");
                for(int v=1; v <= version; v++)
                    await DeleteInstance(context, payload.BaseUrl, payload.ExtensionCode, $"@{nameof(CaseRefreshLogsDurableEntity).ToLower()}@{$"{caseId}-{v}"}");
            }
            catch (Exception exception)
            {
                log.LogMethodError(payload.CorrelationId, loggingName, $"Error when running {nameof(DeleteCaseOrchestrator)} orchestration with id '{context.InstanceId}'", exception);
                throw;
            }
            finally
            {
                log.LogMethodExit(payload.CorrelationId, loggingName, string.Empty);
            }
        }

        private async Task<DurableHttpResponse> DeleteInstance(IDurableOrchestrationContext context, string baseUrl, string extensionCode, string instanceId)
        {
            var instancePath = RestApi.GetInstancePath(instanceId);
            var url = $"{baseUrl}/{instancePath}?code={extensionCode}";
            var uri = new Uri(url);
            var response = await context.CallHttpAsync(HttpMethod.Delete, uri);

            return response;
        }
    }
}