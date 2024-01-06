using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Common.Configuration;
using Common.Logging;
using Common.Validators.Contracts;
using Gateway.Clients.PolarisPipeline.Contracts;
using Common.Telemetry.Wrappers.Contracts;
using Common.ValueObjects;

namespace PolarisGateway.Functions.PolarisPipeline.Document
{
    public class PolarisPipelineGetDocumentSasUrl : BasePolarisFunction
    {
        private readonly IPipelineClient _pipelineClient;
        private readonly ILogger<PolarisPipelineGetDocumentSasUrl> _logger;

        const string loggingName = $"{nameof(PolarisPipelineGetDocumentSasUrl)} - {nameof(Run)}";

        public PolarisPipelineGetDocumentSasUrl(IAuthorizationValidator tokenValidator,
                                                ILogger<PolarisPipelineGetDocumentSasUrl> logger,
                                                IPipelineClient pipelineClient,
                                                ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
            : base(logger, tokenValidator, telemetryAugmentationWrapper)
        {
            _pipelineClient = pipelineClient ?? throw new ArgumentNullException(nameof(pipelineClient));
            _logger = logger;
        }

        [FunctionName(nameof(PolarisPipelineGetDocumentSasUrl))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.DocumentSasUrl)] HttpRequest req, string caseUrn, int caseId, string polarisDocumentId)
        {
            Guid currentCorrelationId = default;

            try
            {
                var request = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                if (request.InvalidResponseResult != null)
                {
                    return request.InvalidResponseResult;
                }

                currentCorrelationId = request.CurrentCorrelationId;

                if (string.IsNullOrWhiteSpace(caseUrn))
                {
                    return BadRequestErrorResponse("Urn is not supplied.", currentCorrelationId, loggingName);
                }

                var sasUrl = await _pipelineClient.GenerateDocumentSasUrlAsync(caseUrn, caseId, new PolarisDocumentId(polarisDocumentId), currentCorrelationId);

                return !string.IsNullOrWhiteSpace(sasUrl)
                    ? new OkObjectResult(sasUrl)
                    : NotFoundErrorResponse($"No document SAS URL found for urn {caseUrn}, caseId {caseId}, polarisDocumentId {polarisDocumentId}\".", currentCorrelationId, loggingName);
            }
            catch (Exception exception)
            {
                return exception switch
                {
                    HttpRequestException => InternalServerErrorResponse(exception, $"A pipeline client http exception occurred when calling {nameof(_pipelineClient.GenerateDocumentSasUrlAsync)}.", currentCorrelationId, loggingName),
                    _ => InternalServerErrorResponse(exception, "An unhandled exception occurred.", currentCorrelationId, loggingName)
                };
            }
        }
    }
}
