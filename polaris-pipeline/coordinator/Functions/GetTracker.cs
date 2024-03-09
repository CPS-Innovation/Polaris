using System;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Extensions;
using Common.Wrappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using coordinator.Functions.DurableEntity.Entity.Mapper;
using coordinator.Durable.Orchestration;
using coordinator.Durable.Entity;
using Microsoft.AspNetCore.Http;
using coordinator.Helpers;

namespace coordinator.Functions
{
    public class GetTracker
    {
        const string loggingName = $"{nameof(GetTracker)} - {nameof(HttpStart)}";
        const string correlationErrorMessage = "Invalid correlationId. A valid GUID is required.";

        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly ICaseDurableEntityMapper _caseDurableEntityMapper;
        private readonly ILogger<GetTracker> _logger;

        public GetTracker(IJsonConvertWrapper jsonConvertWrapper, ICaseDurableEntityMapper caseDurableEntityMapper, ILogger<GetTracker> logger)
        {
            _jsonConvertWrapper = jsonConvertWrapper;
            _caseDurableEntityMapper = caseDurableEntityMapper;
            _logger = logger;
        }

        [FunctionName(nameof(GetTracker))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = RestApi.CaseTracker)] HttpRequest req,
            string caseUrn,
            string caseId,
            [DurableClient] IDurableEntityClient client)
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();

                // todo: temporary code
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
                    return new NotFoundObjectResult($"No Case Entity found with id '{caseId}' with exception '{ex.GetType().Name}: {ex.Message}");
                }

                if (!caseEntity.EntityExists)
                {
                    return new NotFoundObjectResult($"No Case Entity found with id '{caseId}'");
                }

                var trackerDto = _caseDurableEntityMapper.MapCase(caseEntity.EntityState);
                return new OkObjectResult(trackerDto);
            }
            catch (Exception ex)
            {
                return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(GetTracker), currentCorrelationId, ex);
            }
        }
    }
}
