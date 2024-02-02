using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Constants;
using Common.Logging;
using Common.Wrappers.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using coordinator.Functions.DurableEntity.Entity;
using coordinator.Functions.Orchestration;
using coordinator.Mappers;

namespace coordinator.Functions.DurableEntity
{
    public class TrackerClient : BaseClient
    {
        const string correlationErrorMessage = "Invalid correlationId. A valid GUID is required.";
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly ICaseDurableEntityMapper _caseDurableEntityMapper;

        public TrackerClient(IJsonConvertWrapper jsonConvertWrapper, ICaseDurableEntityMapper caseDurableEntityMapper)
        {
            _jsonConvertWrapper = jsonConvertWrapper;
            _caseDurableEntityMapper = caseDurableEntityMapper;
        }

        [FunctionName(nameof(TrackerClient))]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = RestApi.CaseTracker)] HttpRequestMessage req,
            string caseUrn,
            string caseId,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            Guid currentCorrelationId = default;

            try
            {
                req.Headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIdValues);
                if (correlationIdValues == null)
                {
                    return new BadRequestObjectResult(correlationErrorMessage);
                }

                var correlationId = correlationIdValues.FirstOrDefault();
                if (!Guid.TryParse(correlationId, out currentCorrelationId))
                    if (currentCorrelationId == Guid.Empty)
                    {
                        return new BadRequestObjectResult(correlationErrorMessage);
                    }

                var (caseEntity, errorMessage) = await GetCaseTrackerForEntity(client, caseId);

                if (errorMessage != null)
                {
                    return new NotFoundObjectResult(errorMessage);
                }

                var trackerDto = _caseDurableEntityMapper.MapCase(caseEntity);
                return new OkObjectResult(trackerDto);
            }
            catch (Exception ex)
            {
                log.LogMethodError(currentCorrelationId, nameof(TrackerClient), ex.Message, ex);
                return new StatusCodeResult(500);
            }
        }

        protected async Task<(CaseDurableEntity CaseEntity, string errorMessage)> GetCaseTrackerForEntity
        (
            IDurableEntityClient client,
            string caseId
        )
        {
            var caseEntityKey = RefreshCaseOrchestrator.GetKey(caseId);
            var caseEntityId = new EntityId(nameof(CaseDurableEntity), caseEntityKey);

            EntityStateResponse<CaseDurableEntity> caseEntity = default;
            try
            {
                caseEntity = await client.ReadEntityStateAsync<CaseDurableEntity>(caseEntityId);
            }
            catch (Exception ex)
            {
                // #23618 - Race condition: if a case orchestrator has just been kicked off then there is a possibility that 
                //  the entity calls that create (or reset) the entity are still queued up by the time the UI calls
                //  this endpoint. In this scenario, a StorageException is thrown and we are told the blob does not exist.
                //  AppInsights so far shows the orchestrator eventually executes and the entity becomes available, so
                //  lets just let the caller have the same experience as `!caseEntity.EntityExists`.
                // Note: the first implementation for the fix was to catch StorageException (which was what was in the App Insights logs).
                //  Falling back to catch Exception as we are not sure if the StorageException is the only exception that can be thrown.
                var errorMessage = $"No Case Entity found with id '{caseId}' with exception '{ex.GetType().Name}: {ex.Message}";
                return (null, errorMessage);
            }

            if (!caseEntity.EntityExists)
            {
                var errorMessage = $"No Case Entity found with id '{caseId}'";
                return (null, errorMessage);
            }

            return (caseEntity.EntityState, null);
        }
    }
}
