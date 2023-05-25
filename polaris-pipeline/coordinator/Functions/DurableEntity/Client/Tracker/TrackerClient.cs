using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Constants;
using Common.Domain.Exceptions;
using Common.Logging;
using Common.Wrappers.Contracts;
using Common.Dto.Tracker;
using coordinator.Functions.DurableEntity.Entity;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.DurableEntity.Client.Tracker
{
    public class TrackerClient
    {
        const string loggingName = $"{nameof(TrackerClient)} - {nameof(HttpStart)}";
        const string correlationErrorMessage = "Invalid correlationId. A valid GUID is required.";

        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public TrackerClient(IJsonConvertWrapper jsonConvertWrapper)
        {
            _jsonConvertWrapper = jsonConvertWrapper;
        }

        [FunctionName(nameof(TrackerClient))]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", "put", Route = RestApi.CaseTracker)] HttpRequestMessage req,
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

                var entityId = new EntityId(nameof(CaseTrackerEntity), caseId);
                var trackerState = await client.ReadEntityStateAsync<CaseTrackerEntity>(entityId);

                if (!trackerState.EntityExists)
                {
                    var baseMessage = $"No pipeline tracker found with id '{caseId}'";
                    log.LogMethodFlow(currentCorrelationId, loggingName, baseMessage);
                    return new NotFoundObjectResult(baseMessage);
                }

                switch (req.Method.Method)
                {
                    case "GET":
                        log.LogMethodExit(currentCorrelationId, loggingName, string.Empty);
                        var trackerEntity = trackerState.EntityState;
                        var trackerDto = trackerEntity.Adapt<TrackerDto>();
                        return new OkObjectResult(trackerDto);

                    default:
                        throw new BadRequestException("Unexpected HTTP Verb", req.Method.Method);
                }
            }
            catch (Exception ex)
            {
                log.LogMethodError(currentCorrelationId, loggingName, ex.Message, ex);
                return new StatusCodeResult(500);
            }
        }
    }
}
