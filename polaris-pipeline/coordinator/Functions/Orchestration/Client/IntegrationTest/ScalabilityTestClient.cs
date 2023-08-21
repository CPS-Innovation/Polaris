#if SCALABILITY_TEST
using Common.Configuration;
using Common.Domain.Exceptions;
using Common.Logging;
using Common.Services.CaseSearchService.Contracts;
using Common.Telemetry.Contracts;
using Common.Wrappers.Contracts;
using coordinator.Domain;
using coordinator.Functions.Orchestration.Functions.Case;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace coordinator.Functions.Orchestration.Client.Case
{
    public class ScalabilityTestClient
    {
        private readonly ISearchIndexService _searchIndexService;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly ILogger<CaseClient> _logger;
        private readonly ITelemetryClient _telemetryClient;
        private const string LoggingName = $"{nameof(CaseClient)} - {nameof(Run)}";
        private const int DefaultPageSize = 100;

        private OrchestrationRuntimeStatus[] terminateStatuses = new OrchestrationRuntimeStatus[]
        {
            OrchestrationRuntimeStatus.Running,
            OrchestrationRuntimeStatus.Pending,
            OrchestrationRuntimeStatus.Suspended,
            OrchestrationRuntimeStatus.ContinuedAsNew
        };

        private OrchestrationRuntimeStatus[] purgeStatuses = new OrchestrationRuntimeStatus[]
        {
            OrchestrationRuntimeStatus.Completed,
            OrchestrationRuntimeStatus.Canceled,
            OrchestrationRuntimeStatus.Failed,
            OrchestrationRuntimeStatus.Terminated
        };

        public ScalabilityTestClient(
            ISearchIndexService searchIndexService,
            IJsonConvertWrapper jsonConvertWrapper,
            ILogger<CaseClient> logger,
            ITelemetryClient telemetryClient)
        {
            _searchIndexService = searchIndexService;
            _jsonConvertWrapper = jsonConvertWrapper;
            _logger = logger;
            _telemetryClient = telemetryClient;
        }

        [FunctionName(nameof(ScalabilityTestClient))]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.Locked)] // Refresh already running
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<HttpResponseMessage> Run
            (
                [HttpTrigger(AuthorizationLevel.Anonymous, "delete", "post", Route = RestApi.ScalabilityTest)] HttpRequestMessage req,
                long caseId,
                int documentCount,
                [DurableClient] IDurableOrchestrationClient orchestrationClient
            )
        {
            Guid currentCorrelationId = default;
            const string loggingName = $"{nameof(ScalabilityTestClient)} - {nameof(Run)}";
            var baseUrl = req.RequestUri.GetLeftPart(UriPartial.Authority);

            try
            {
                ScalabilityTestCaseOrchestrationPayload payload = new ScalabilityTestCaseOrchestrationPayload(caseId, documentCount);

                switch (req.Method.Method)
                {
                    case "POST":
                        var instanceName = $"ScalabilityTest-{caseId}";
                        var existingInstance = await orchestrationClient.GetStatusAsync(instanceName);
                        bool isSingletonRefreshRunning = IsSingletonRefreshRunning(existingInstance);
                        var instanceId = ScalabilityTestCaseOrchestrator.GetKey(caseId);
                        await orchestrationClient.StartNewAsync(nameof(ScalabilityTestCaseOrchestrator), instanceId, payload);

                        return orchestrationClient.CreateCheckStatusResponse(req, instanceId);

                    case "DELETE":
                        var startTime = DateTime.UtcNow;

                        var terminateConditions = GetOrchestrationQueries(terminateStatuses, caseId);
                        var instanceIds = await TerminateOrchestrationsAndDurableEntities(orchestrationClient, terminateConditions, currentCorrelationId);
                        var gotTerminateInstancesTime = DateTime.UtcNow;

                        await WaitForOrchestrationsToTerminateTask(orchestrationClient, instanceIds);
                        var terminatedInstancesTime = DateTime.UtcNow;

                        var purgeConditions = GetOrchestrationQueries(purgeStatuses, caseId);
                        var success = await PurgeOrchestrationsAndDurableEntities(orchestrationClient, purgeConditions, currentCorrelationId);
                        var purgedInstancesTime = DateTime.UtcNow;

                        return new HttpResponseMessage(success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError);

                    default:
                        throw new BadRequestException("Unexpected HTTP Verb", req.Method.Method);
                }
            }
            catch (Exception exception)
            {
                var rootCauseMessage = "An unhandled exception occurred";
                var httpStatusCode = HttpStatusCode.InternalServerError;

                if (exception is UnauthorizedException)
                {
                    rootCauseMessage = "Unauthorized";
                    httpStatusCode = HttpStatusCode.Unauthorized;
                }
                else if (exception is BadRequestException)
                {
                    rootCauseMessage = "Invalid request";
                    httpStatusCode = HttpStatusCode.BadRequest;
                }

                var errorMessage = $"{rootCauseMessage}. {exception.Message}.  Base exception message: {exception.GetBaseException().Message}";

                _logger.LogMethodError(currentCorrelationId, loggingName, errorMessage, exception);

                return new HttpResponseMessage(httpStatusCode)
                {
                    Content = new StringContent(errorMessage, Encoding.UTF8, MediaTypeNames.Application.Json)
                };
            }
            finally
            {
                _logger.LogMethodExit(currentCorrelationId, loggingName, "n/a");
            }
        }

        private async Task<List<string>> TerminateOrchestrationsAndDurableEntities(IDurableOrchestrationClient client, List<OrchestrationStatusQueryCondition> terminateConditions, Guid correlationId)
        {
            _logger.LogMethodFlow(correlationId, LoggingName, "Terminating Case Orchestrations, Sub Orchestrations and Durable Entities");

            var instanceIds = new List<string>();
            foreach (var terminateCondition in terminateConditions)
            {
                do
                {
                    var statusQueryResult = await client.ListInstancesAsync(terminateCondition, CancellationToken.None);
                    terminateCondition.ContinuationToken = statusQueryResult.ContinuationToken;

                    var instancesToTerminate = statusQueryResult.DurableOrchestrationState.Select(o => o.InstanceId).ToList();
                    instanceIds.AddRange(instancesToTerminate);

                    await Task.WhenAll(instancesToTerminate.Select(async instanceId => await client.TerminateAsync(instanceId, "Forcibly terminated DELETE")));
                }
                while (terminateCondition.ContinuationToken != null);
            }

            var success = await WaitForOrchestrationsToTerminateTask(client, instanceIds);

            _logger.LogMethodFlow(correlationId, LoggingName, $"Terminating Case Orchestrations, Sub Orchestrations and Durable Entities completed");

            return instanceIds;
        }

        private async Task<bool> WaitForOrchestrationsToTerminateTask(IDurableOrchestrationClient client, List<string> instanceIds)
        {
            const int totalWaitTimeSeconds = 30;
            const int retryDelayMilliseconds = 1000;

            for (int i = 0; i < (totalWaitTimeSeconds * 1000) / retryDelayMilliseconds; i++)
            {
                var statuses = await client.GetStatusAsync(instanceIds);

                if (statuses == null || statuses.All(statuses => statuses.RuntimeStatus == OrchestrationRuntimeStatus.Terminated))
                    return true;

                await Task.Delay(retryDelayMilliseconds);
            }

            return false;
        }

        private async Task<bool> PurgeOrchestrationsAndDurableEntities(IDurableOrchestrationClient client, List<OrchestrationStatusQueryCondition> purgeConditions, Guid correlationId)
        {
            _logger.LogMethodFlow(correlationId, LoggingName, "Purging Case Orchestrations, Sub Orchestrations and Durable Entities");

            foreach (var purgeCondition in purgeConditions)
            {
                do
                {
                    var statusQueryResult = await client.ListInstancesAsync(purgeCondition, CancellationToken.None);
                    purgeCondition.ContinuationToken = statusQueryResult.ContinuationToken;

                    var instancesToPurge = statusQueryResult.DurableOrchestrationState.Select(o => o.InstanceId);

                    await client.PurgeInstanceHistoryAsync(instancesToPurge);
                } while (purgeCondition.ContinuationToken != null);

            }
            _logger.LogMethodFlow(correlationId, LoggingName, $"Purging Case Orchestrations, Sub Orchestrations and Durable Entities completed");

            return true;
        }

        private static bool IsSingletonRefreshRunning(DurableOrchestrationStatus existingInstance)
        {
            // https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-singletons?tabs=csharp
            bool notRunning = existingInstance == null ||
                    existingInstance.RuntimeStatus
                        is OrchestrationRuntimeStatus.Completed
                        or OrchestrationRuntimeStatus.Failed
                        or OrchestrationRuntimeStatus.Terminated
                        // Is this correct? unit tests assert Canceled state, but MS docs don't include this state
                        or OrchestrationRuntimeStatus.Canceled;

            return !notRunning;
        }

        private static List<OrchestrationStatusQueryCondition> GetOrchestrationQueries(IEnumerable<OrchestrationRuntimeStatus> runtimeStatuses, long caseId)
        {
            var conditions = new List<OrchestrationStatusQueryCondition>();

            /* var caseAndDocumentCondition = new OrchestrationStatusQueryCondition
            {
                InstanceIdPrefix = ScalabilityTestOrchestrator.GetKey(caseId),
                CreatedTimeFrom = DateTime.MinValue,
                CreatedTimeTo = DateTime.UtcNow,
                RuntimeStatus = runtimeStatuses,
                PageSize = DefaultPageSize,
                ContinuationToken = null
            };
            conditions.Add(caseAndDocumentCondition);

            var caseDurableEntityCondition = new OrchestrationStatusQueryCondition
            {
                InstanceIdPrefix = CaseDurableEntity.GetInstanceId(caseId),
                CreatedTimeFrom = DateTime.MinValue,
                CreatedTimeTo = DateTime.UtcNow,
                RuntimeStatus = runtimeStatuses,
                PageSize = DefaultPageSize,
                ContinuationToken = null
            };
            conditions.Add(caseDurableEntityCondition);

            var caseRefreshLogsDurableEntitiesCondition = new OrchestrationStatusQueryCondition
            {
                InstanceIdPrefix = CaseRefreshLogsDurableEntity.GetInstanceId(caseId, 0).Replace("-0", string.Empty),
                CreatedTimeFrom = DateTime.MinValue,
                CreatedTimeTo = DateTime.UtcNow,
                RuntimeStatus = runtimeStatuses,
                PageSize = DefaultPageSize,
                ContinuationToken = null
            };
            conditions.Add(caseRefreshLogsDurableEntitiesCondition); */

            return conditions;
        }
    }
}
#endif