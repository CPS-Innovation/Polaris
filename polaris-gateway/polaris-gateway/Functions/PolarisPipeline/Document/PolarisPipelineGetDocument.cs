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
using Common.ValueObjects;

namespace PolarisGateway.Functions.PolarisPipeline.Document
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Document)] HttpRequest req, string caseUrn, int caseId, string polarisDocumentId)
        {
            Guid currentCorrelationId = default;

            try
            {
                #region Validate-Inputs
                var request = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                if (request.InvalidResponseResult != null)
                    return request.InvalidResponseResult;

                currentCorrelationId = request.CurrentCorrelationId;
                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                if (string.IsNullOrWhiteSpace(caseUrn))
                    return BadRequestErrorResponse("Urn is not supplied.", currentCorrelationId, loggingName);
                #endregion

                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Getting document for urn {caseUrn}, caseId {caseId}, id {polarisDocumentId}");
                var blobStream = await _pipelineClient.GetDocumentAsync(caseUrn, caseId, new PolarisDocumentId(polarisDocumentId), currentCorrelationId);

                return blobStream != null
                                    ? new OkObjectResult(blobStream)
                                    : NotFoundErrorResponse($"No document found for document id '{polarisDocumentId}'.", currentCorrelationId, loggingName);
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

