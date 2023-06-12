using Common.Configuration;
using Common.Constants;
using Common.Domain.Exceptions;
using Common.Logging;
using Common.Wrappers.Contracts;
using coordinator.Domain;
using coordinator.Functions.DurableEntity.Entity;
using coordinator.Functions.Orchestration.Functions.Case;
using coordinator.Functions.Orchestration.Functions.Tracker;
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
using System.Web;

namespace coordinator.Functions.Orchestration.Client.Case
{
    public class CaseClient
    {
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly ILogger<CaseClient> _logger;
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

        public CaseClient(IJsonConvertWrapper jsonConvertWrapper, ILogger<CaseClient> logger)
        {
            _jsonConvertWrapper = jsonConvertWrapper;
            _logger = logger;
        }

        [FunctionName(nameof(CaseClient))]
        [ProducesResponseType((int)HttpStatusCode.Accepted)] 
        [ProducesResponseType((int)HttpStatusCode.Locked)] // Refresh already running
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)] 
        public async Task<HttpResponseMessage> Run
            (
                [HttpTrigger(AuthorizationLevel.Anonymous, "put", "delete", "post", Route = RestApi.Case)] HttpRequestMessage req,
                string caseUrn,
                string caseId,
                [DurableClient] IDurableOrchestrationClient orchestrationClient
            )
        {
            Guid currentCorrelationId = default;
            const string loggingName = $"{nameof(CaseClient)} - {nameof(Run)}";
            var baseUrl = req.RequestUri.GetLeftPart(UriPartial.Authority);
            var extensionCode = HttpUtility.ParseQueryString(req.RequestUri.Query).Get("code");

            try
            {
                #region Validate-Inputs
                req.Headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIdValues);
                if (correlationIdValues == null)
                    throw new BadRequestException("Invalid correlationId. A valid GUID is required.", nameof(req));

                var correlationId = correlationIdValues.FirstOrDefault();
                if (!Guid.TryParse(correlationId, out currentCorrelationId))
                    if (currentCorrelationId == Guid.Empty)
                        throw new BadRequestException("Invalid correlationId. A valid GUID is required.", correlationId);

                req.Headers.TryGetValues(HttpHeaderKeys.CmsAuthValues, out var cmsAuthValuesValues);
                if (cmsAuthValuesValues == null)
                    throw new BadRequestException("Invalid Cms Auth token. A valid Cms Auth token must be received for this request.", nameof(req));
                var cmsAuthValues = cmsAuthValuesValues.First();
                if (string.IsNullOrWhiteSpace(cmsAuthValues))
                    throw new BadRequestException("Invalid Cms Auth token. A valid Cms Auth token must be received for this request.", nameof(req));

                _logger.LogMethodEntry(currentCorrelationId, loggingName, req.RequestUri?.Query);

                if (string.IsNullOrWhiteSpace(caseUrn))
                    throw new BadRequestException("A case URN must be supplied.", caseUrn);

                if (!int.TryParse(caseId, out var caseIdNum))
                    throw new BadRequestException("Invalid case id. A 32-bit integer is required.", caseId);

                if (req.RequestUri == null)
                    throw new BadRequestException("Expected querystring value", nameof(req));
                #endregion

                CaseOrchestrationPayload casePayload = new CaseOrchestrationPayload(caseUrn, caseIdNum, baseUrl, extensionCode, cmsAuthValues, currentCorrelationId);

                switch (req.Method.Method)
                {
                    case "POST":
                        var existingInstance = await orchestrationClient.GetStatusAsync(caseId);
                        bool isSingletonRefreshRunning = IsSingletonRefreshRunning(existingInstance);

                        if (isSingletonRefreshRunning)
                        {
                            _logger.LogMethodFlow(currentCorrelationId, loggingName, $"{nameof(CaseClient)} Locked as already running - {nameof(RefreshCaseOrchestrator)} with instance id '{caseId}'");
                            return new HttpResponseMessage(HttpStatusCode.Locked);
                        }

                        var instanceId = RefreshCaseOrchestrator.GetKey(caseId);

                        await orchestrationClient.StartNewAsync(nameof(RefreshCaseOrchestrator), instanceId, casePayload);

                        _logger.LogMethodFlow(currentCorrelationId, loggingName, $"{nameof(CaseClient)} Succeeded - Started {nameof(RefreshCaseOrchestrator)} with instance id '{instanceId}'");
                        return orchestrationClient.CreateCheckStatusResponse(req, instanceId);

                    case "DELETE":
                        var terminateConditions = GetOrchestrationQueries(terminateStatuses, caseId);
                        var instanceIds = await TerminateOrchestrationsAndDurableEntities(orchestrationClient, terminateConditions, currentCorrelationId);

                        await WaitForOrchestrationsToTerminateTask(orchestrationClient, instanceIds);

                        var purgeConditions = GetOrchestrationQueries(purgeStatuses, caseId);
                        var success = await PurgeOrchestrationsAndDurableEntities(orchestrationClient, purgeConditions, currentCorrelationId);

                        return new HttpResponseMessage(success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError);

                    case "PUT":
                        var content = await req.Content.ReadAsStringAsync();
                        if (string.IsNullOrWhiteSpace(content))
                        {
                            throw new BadRequestException("Request body cannot be null.", nameof(req));
                        }
                        var tracker = _jsonConvertWrapper.DeserializeObject<CaseDurableEntity>(content);

                        UpdateCaseDurableEntityPayload updateTrackerPayload = new UpdateCaseDurableEntityPayload
                        {
                            CaseOrchestrationPayload = casePayload,
                            Tracker = tracker
                        };

                        await orchestrationClient.StartNewAsync(nameof(UpdateTrackerOrchestrator), caseId, updateTrackerPayload);

                        return new HttpResponseMessage(HttpStatusCode.Accepted);

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
            foreach(var terminateCondition in terminateConditions)
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

            for(int i=0; i < (totalWaitTimeSeconds * 1000) / retryDelayMilliseconds; i++)
            {
                var statuses = await client.GetStatusAsync(instanceIds);

                if(statuses.All(statuses => statuses.RuntimeStatus == OrchestrationRuntimeStatus.Terminated))
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

        private static List<OrchestrationStatusQueryCondition> GetOrchestrationQueries(IEnumerable<OrchestrationRuntimeStatus> runtimeStatuses, string caseId)
        {
            var conditions = new List<OrchestrationStatusQueryCondition>();

            var caseAndDocumentCondition = new OrchestrationStatusQueryCondition
            {
                InstanceIdPrefix = RefreshCaseOrchestrator.GetKey(caseId),
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
            conditions.Add(caseRefreshLogsDurableEntitiesCondition);

            return conditions;
        }
    }
}
