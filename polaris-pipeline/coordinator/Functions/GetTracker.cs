using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Constants;
using Common.Domain.Exceptions;
using Common.Logging;
using Common.Wrappers.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using coordinator.Functions.DurableEntity.Entity.Mapper;

namespace coordinator.Functions
{
    public class GetTracker : BaseClient
    {
        const string loggingName = $"{nameof(GetTracker)} - {nameof(HttpStart)}";
        const string correlationErrorMessage = "Invalid correlationId. A valid GUID is required.";

        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly ICaseDurableEntityMapper _caseDurableEntityMapper;

        public GetTracker(IJsonConvertWrapper jsonConvertWrapper, ICaseDurableEntityMapper caseDurableEntityMapper)
        {
            _jsonConvertWrapper = jsonConvertWrapper;
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
                log.LogMethodError(currentCorrelationId, loggingName, ex.Message, ex);
                return new StatusCodeResult(500);
            }
        }
    }
}
