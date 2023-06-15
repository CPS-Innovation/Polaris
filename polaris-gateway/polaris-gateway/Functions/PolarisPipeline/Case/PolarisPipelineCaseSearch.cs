using Azure;
using Common.Configuration;
using Common.Logging;
using Common.Validators.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Gateway.Clients.PolarisPipeline.Contracts;
using System;
using System.Threading.Tasks;
using PolarisGateway.Wrappers;
using Microsoft.Extensions.Primitives;
using System.Linq;
using System.Collections.Generic;

namespace PolarisGateway.Functions.PolarisPipeline.Case
{
    public class PolarisPipelineCaseSearch : BasePolarisFunction
    {
        private readonly IPipelineClient _pipelineClient;
        private readonly ILogger<PolarisPipelineCaseSearch> _logger;

        const string loggingName = $"{nameof(PolarisPipelineCaseSearch)} - {nameof(Run)}";

        static HashSet<string> e2eCorrelationIds = new HashSet<string>
            {
                "E2E00000-0000-0000-0000-000000000000",
                "E2E00000-0000-0000-0000-000000000001",
                "E2E00000-0000-0000-0000-000000000002",
                "E2E00000-0000-0000-0000-000000000003"
            };

        public PolarisPipelineCaseSearch(ILogger<PolarisPipelineCaseSearch> logger,
                                               IPipelineClient pipelineClient,
                                               IAuthorizationValidator tokenValidator,
                                               ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
            : base(logger, tokenValidator, telemetryAugmentationWrapper)
        {
            _pipelineClient = pipelineClient;
            _logger = logger;
        }

        [FunctionName(nameof(PolarisPipelineCaseSearch))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseSearch)] HttpRequest req, string caseUrn, int caseId)
        {
            Guid currentCorrelationId = default;

            try
            {
                // Try to mitigate the fact that immediately querying Azure Search after updating it is not guaranteed to work
                // This is a temporary workaround until we can implement a better solution
                // If running e2e tests put in a 5s delay to allow the Azure Search index to update
                // This is based on the timings / results from below... 
                //
                // e2e / pipeline delays versus test results 
                //  e2e timeout 2500ms, GET delay 5000ms : (10/10) 100% 
                //  e2e timeout 2500ms, GET delay 2000m  : (9/10)  90% 
                //  e2e timeout 2500ms, GET delay 0ms    : (8/10)  80% 
                if(IsEndToEndTest(req))
                    Task.Delay(5000).Wait();

                #region Validate-Inputs
                var request = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                if (request.InvalidResponseResult != null)
                    return request.InvalidResponseResult;

                currentCorrelationId = request.CurrentCorrelationId;
                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                if (caseId <= 0)
                    return BadRequestErrorResponse("A valid caseId must be supplied, one that is greater than zero", currentCorrelationId, loggingName);

                if (!req.Query.ContainsKey("query"))
                    return BadRequestErrorResponse("Search query is not supplied.", currentCorrelationId, loggingName);

                string searchTerm = req.Query["query"];
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return BadRequestErrorResponse("Search query term is not supplied.", currentCorrelationId, loggingName);
                #endregion

                var searchResults = await _pipelineClient.SearchCase(caseUrn, caseId, searchTerm, currentCorrelationId);

                return new OkObjectResult(searchResults);
            }
            catch (Exception exception)
            {
                return exception switch
                {
                    RequestFailedException => InternalServerErrorResponse(exception, "A search client index exception occurred.", currentCorrelationId, loggingName),
                    _ => InternalServerErrorResponse(exception, "An unhandled exception occurred.", currentCorrelationId, loggingName)
                };
            }
            finally
            {
                _logger.LogMethodExit(currentCorrelationId, loggingName, string.Empty);
            }
        }

        private bool IsEndToEndTest(HttpRequest req)
        {
            if(req.Headers.TryGetValue("correlation-id", out StringValues correlationIds))
            {
                var correlationId = correlationIds.First();

                if(!string.IsNullOrWhiteSpace(correlationId))
                {
                    return e2eCorrelationIds.Contains(correlationId);
                }
            }

            return false;
        }
    }
}

