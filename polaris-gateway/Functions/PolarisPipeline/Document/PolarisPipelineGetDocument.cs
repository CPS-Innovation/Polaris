using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Telemetry.Wrappers.Contracts;
using PolarisGateway.Domain.Validators;
using Common.Configuration;
using Common.ValueObjects;
using Gateway.Clients;
using Common.Extensions;
using System.Net;

namespace PolarisGateway.Functions.PolarisPipeline.Document
{
    public class PolarisPipelineGetDocument : BasePolarisFunction
    {
        private readonly IPipelineClient _pipelineClient;
        private readonly ILogger<PolarisPipelineGetDocument> _logger;
        private const string PdfContentType = "application/pdf";

        public PolarisPipelineGetDocument(IPipelineClient pipelineClient,
                                          ILogger<PolarisPipelineGetDocument> logger,
                                          IAuthorizationValidator tokenValidator,
                                          ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
        : base(logger, tokenValidator, telemetryAugmentationWrapper)
        {
            _pipelineClient = pipelineClient;
            _logger = logger;
        }

        [FunctionName(nameof(PolarisPipelineGetDocument))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Document)] HttpRequest req, string caseUrn, int caseId, string polarisDocumentId)
        {
            try
            {
                await Initiate(req);

                var result = await _pipelineClient.GetDocumentAsync(caseUrn, caseId, new PolarisDocumentId(polarisDocumentId), CorrelationId);
                return new FileStreamResult(result, PdfContentType);
            }
            catch (Exception exception)
            {
                if (exception is HttpRequestException h && h.StatusCode == HttpStatusCode.NotFound)
                {
                    return new NotFoundObjectResult($"No tracker found for case Urn '{caseUrn}', case id '{caseId}'.");
                }
                return HandleUnhandledException(exception);
            }
        }
    }
}

