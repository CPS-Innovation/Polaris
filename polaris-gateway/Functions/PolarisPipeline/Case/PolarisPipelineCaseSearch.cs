using Common.Configuration;
using PolarisGateway.Domain.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Gateway.Clients;
using Common.Telemetry.Wrappers.Contracts;
using Common.Domain.Exceptions;

namespace PolarisGateway.Functions.PolarisPipeline.Case
{
    public class PolarisPipelineCaseSearch : BasePolarisFunction
    {
        private readonly IPipelineClient _pipelineClient;
        private readonly ILogger<PolarisPipelineCaseSearch> _logger;

        private const string Query = "query";

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
            try
            {
                await Initiate(req);

                string searchTerm = req.Query[Query];
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    throw new BadRequestException("Search query term is not supplied.", Query);
                }

                var result = await _pipelineClient.SearchCase(caseUrn, caseId, searchTerm, CorrelationId);

                return new OkObjectResult(result);
            }
            catch (Exception exception)
            {
                return HandleUnhandledException(exception);
            }
        }
    }
}

