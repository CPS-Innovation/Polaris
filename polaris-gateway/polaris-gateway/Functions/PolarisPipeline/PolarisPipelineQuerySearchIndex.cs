using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PolarisGateway.Clients.PolarisPipeline;
using System;
using System.Threading.Tasks;
using PolarisGateway.Domain.Logging;
using PolarisGateway.Domain.Validators;

namespace PolarisGateway.Functions.PolarisPipeline
{
    public class PolarisPipelineQuerySearchIndex : BasePolarisFunction
    {
        private readonly ISearchIndexClient _searchIndexClient;
        private readonly ILogger<PolarisPipelineQuerySearchIndex> _logger;

        public PolarisPipelineQuerySearchIndex(ILogger<PolarisPipelineQuerySearchIndex> logger, ISearchIndexClient searchIndexClient, IAuthorizationValidator tokenValidator)
            : base(logger, tokenValidator)
        {
            _searchIndexClient = searchIndexClient;
            _logger = logger;
        }

        [FunctionName("PolarisPipelineQuerySearchIndex")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "urns/{urn}/cases/{caseId}/query/{*searchTerm}")] HttpRequest req, int caseId, string searchTerm)
        {
            Guid currentCorrelationId = default;
            const string loggingName = "PolarisPipelineQuerySearchIndex - Run";

            try
            {
                var validationResult = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                if (validationResult.InvalidResponseResult != null)
                    return validationResult.InvalidResponseResult;

                currentCorrelationId = validationResult.CurrentCorrelationId;
                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                if (caseId <= 0)
                    return BadRequestErrorResponse("A valid caseId must be supplied, one that is greater than zero", currentCorrelationId, loggingName);

                if (string.IsNullOrWhiteSpace(searchTerm))
                    return BadRequestErrorResponse("Search term is not supplied.", currentCorrelationId, loggingName);

                var searchResults = await _searchIndexClient.Query(caseId, searchTerm, currentCorrelationId);

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

