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
using PolarisGateway.Domain.Logging;
using PolarisGateway.Domain.Validators;
using PolarisGateway.Wrappers;

namespace PolarisGateway.Functions.PolarisPipeline
{
    public class PolarisPipelineQuerySearchIndex : BasePolarisFunction
    {
        private readonly IPipelineClient _pipelineClient;
        private readonly ILogger<PolarisPipelineQuerySearchIndex> _logger;

        public PolarisPipelineQuerySearchIndex(ILogger<PolarisPipelineQuerySearchIndex> logger,
                                               IPipelineClient pipelineClient,
                                               IAuthorizationValidator tokenValidator,
                                               ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
            : base(logger, tokenValidator, telemetryAugmentationWrapper)
        {
            _pipelineClient = pipelineClient;
            _logger = logger;
        }

        [FunctionName(nameof(PolarisPipelineQuerySearchIndex))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.DocumentsSearch)] HttpRequest req, string caseUrn, int caseId)
        {
            Guid currentCorrelationId = default;

            try
            {
                var request = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                if (request.InvalidResponseResult != null)
                    return request.InvalidResponseResult;

                currentCorrelationId = request.CurrentCorrelationId;
                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                if (caseId <= 0)
                    return BadRequestErrorResponse("A valid caseId must be supplied, one that is greater than zero", currentCorrelationId, loggingName);

                string searchTerm;
                if (!req.Query.ContainsKey("query"))
                    return BadRequestErrorResponse("Search query is not supplied.", currentCorrelationId, loggingName);

                searchTerm = req.Query["query"];
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return BadRequestErrorResponse("Search query term is not supplied.", currentCorrelationId, loggingName);

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
    }
}

