using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Constants;
using Common.Logging;
using coordinator.Domain.Tracker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.ClientFunctions.Case
{
    public class GetTrackerStatus
    {
        const string loggingName = $"{nameof(GetTrackerStatus)} - {nameof(HttpStart)}";
        const string correlationErrorMessage = "Invalid correlationId. A valid GUID is required.";

        [FunctionName(nameof(GetTrackerStatus))]
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
                    log.LogMethodFlow(Guid.Empty, loggingName, correlationErrorMessage);
                    return new BadRequestObjectResult(correlationErrorMessage);
                }

                var correlationId = correlationIdValues.FirstOrDefault();
                if (!Guid.TryParse(correlationId, out currentCorrelationId))
                    if (currentCorrelationId == Guid.Empty)
                    {
                        log.LogMethodFlow(Guid.Empty, loggingName, correlationErrorMessage);
                        return new BadRequestObjectResult(correlationErrorMessage);
                    }

                log.LogMethodEntry(currentCorrelationId, loggingName, caseId);

                var entityId = new EntityId(nameof(Domain.Tracker), caseId);
                var stateResponse = await client.ReadEntityStateAsync<Tracker>(entityId);
                if (!stateResponse.EntityExists)
                {
                    var baseMessage = $"No pipeline tracker found with id '{caseId}'";
                    log.LogMethodFlow(currentCorrelationId, loggingName, baseMessage);
                    return new NotFoundObjectResult(baseMessage);
                }

                log.LogMethodExit(currentCorrelationId, loggingName, string.Empty);
                return new OkObjectResult(stateResponse.EntityState);
            }
            catch (Exception ex)
            {
                log.LogMethodError(currentCorrelationId, loggingName, ex.Message, ex);
                return new StatusCodeResult(500);
            }

        }
    }
}
