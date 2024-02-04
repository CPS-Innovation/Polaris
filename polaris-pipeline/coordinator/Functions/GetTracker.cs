using System;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using coordinator.Durable.Entity;
using coordinator.Mappers;
using Common.Extensions;

namespace coordinator.Functions
{
    public class GetTracker
    {
        private readonly ICaseDurableEntityMapper _caseDurableEntityMapper;

        public GetTracker(ICaseDurableEntityMapper caseDurableEntityMapper)
        {
            _caseDurableEntityMapper = caseDurableEntityMapper;
        }

        [FunctionName(nameof(GetTracker))]
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
                currentCorrelationId = req.Headers.GetCorrelationId();

                CaseDurableEntity caseEntity;
                try
                {
                    caseEntity = await CaseDurableEntity.GetReadOnlyEntityState(client, caseId);
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
                    return new NotFoundObjectResult($"No Case Entity found with id '{caseId}' with exception '{ex.GetType().Name}: {ex.Message}");
                }

                // todo: we should be confident that caseEntity is not null here and remove the ?? clause.
                var trackerDto = _caseDurableEntityMapper.MapCase(caseEntity);
                return new OkObjectResult(trackerDto);
            }
            catch (Exception ex)
            {
                log.LogMethodError(currentCorrelationId, nameof(GetTracker), ex.Message, ex);
                return new StatusCodeResult(500);
            }
        }

    }
}
