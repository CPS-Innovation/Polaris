using Azure;
using Common.Configuration;
using PolarisGateway.Domain.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Gateway.Clients;
using Common.Telemetry.Wrappers.Contracts;

namespace PolarisGateway.Functions.PolarisPipeline.Case
{
    public class PolarisPipelineCaseSearch : BasePolarisFunction
    {
        private readonly IPipelineClient _pipelineClient;
        private readonly ILogger<PolarisPipelineCaseSearch> _logger;

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
                var request = await ValidateRequest(req, nameof(PolarisPipelineCaseSearch), ValidRoles.UserImpersonation);
                if (request.InvalidResponseResult != null)
                    return request.InvalidResponseResult;

                currentCorrelationId = request.CurrentCorrelationId;

                if (caseId <= 0)
                    return BadRequestErrorResponse("A valid caseId must be supplied, one that is greater than zero", currentCorrelationId, nameof(PolarisPipelineCaseSearch));

                if (!req.Query.ContainsKey("query"))
                    return BadRequestErrorResponse("Search query is not supplied.", currentCorrelationId, nameof(PolarisPipelineCaseSearch));

                string searchTerm = req.Query["query"];
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return BadRequestErrorResponse("Search query term is not supplied.", currentCorrelationId, nameof(PolarisPipelineCaseSearch));

                var searchResults = await _pipelineClient.SearchCase(caseUrn, caseId, searchTerm, currentCorrelationId);

                return new OkObjectResult(searchResults);
            }
            catch (Exception exception)
            {
                return exception switch
                {
                    RequestFailedException => InternalServerErrorResponse(exception, "A search client index exception occurred.", currentCorrelationId, nameof(PolarisPipelineCaseSearch)),
                    _ => InternalServerErrorResponse(exception, "An unhandled exception occurred.", currentCorrelationId, nameof(PolarisPipelineCaseSearch))
                };
            }
        }
    }
}

