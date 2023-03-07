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
        private readonly IPipelineClient _pipelineClient;
        private readonly ILogger<PolarisPipelineQuerySearchIndex> _logger;

        public PolarisPipelineQuerySearchIndex(ILogger<PolarisPipelineQuerySearchIndex> logger, IPipelineClient pipelineClient, IAuthorizationValidator tokenValidator)
            : base(logger, tokenValidator)
        {
            _pipelineClient = pipelineClient;
            _logger = logger;
        }

        [FunctionName("PolarisPipelineQuerySearchIndex")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, 
            "get", 
            Route = "urns/{urn}/cases/{caseId}/documents/search")] HttpRequest req, 
            string urn, int caseId)
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

                string searchTerm;
                if (!req.Query.ContainsKey("query"))
                    return BadRequestErrorResponse("Search query is not supplied.", currentCorrelationId, loggingName);

                searchTerm = req.Query["query"];
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return BadRequestErrorResponse("Search query term is not supplied.", currentCorrelationId, loggingName);

                var searchResults = await _pipelineClient.SearchCase(urn, caseId, searchTerm, currentCorrelationId);

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

