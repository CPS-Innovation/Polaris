using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using PolarisGateway.Wrappers;
using Common.Logging;
using Common.Validators.Contracts;
using Gateway.Clients.PolarisPipeline.Contracts;
using Common.Configuration;

namespace PolarisGateway.Functions.PolarisPipeline
{
    public class PolarisPipelineGetDocument : BasePolarisFunction
    {
        private readonly IPipelineClient _pipelineClient;
        private readonly ILogger<PolarisPipelineGetDocument> _logger;

        const string loggingName = $"{nameof(PolarisPipelineGetDocument)} - ${nameof(Run)}";


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
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Document)] HttpRequest req, string caseUrn, int caseId, Guid documentId)
        {
            Guid currentCorrelationId = default;

            try
            {
                var request = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                if (request.InvalidResponseResult != null)
                    return request.InvalidResponseResult;

                currentCorrelationId = request.CurrentCorrelationId;
                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                if (string.IsNullOrWhiteSpace(caseUrn))
                    return BadRequestErrorResponse("Urn is not supplied.", currentCorrelationId, loggingName);

                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Getting document for urn {caseUrn}, caseId {caseId}, id {documentId}");
                var blobStream = await _pipelineClient.GetDocumentAsync(caseUrn, caseId, documentId, currentCorrelationId);

                return blobStream != null
                                    ? new OkObjectResult(blobStream)
                                    : NotFoundErrorResponse($"No document found for document id '{documentId}'.", currentCorrelationId, loggingName);
            }
            catch (Exception exception)
            {
                return exception switch
                {
                    HttpRequestException => InternalServerErrorResponse(exception, $"A pipeline client http exception occurred when calling {nameof(_pipelineClient.GetDocumentAsync)}.", currentCorrelationId, loggingName),
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

