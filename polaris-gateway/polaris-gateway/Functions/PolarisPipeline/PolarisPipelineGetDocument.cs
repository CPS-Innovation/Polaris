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
using System.Net.Http;
using PolarisGateway.Wrappers;

namespace PolarisGateway.Functions.PolarisPipeline
{
    public class PolarisPipelineGetDocument : BasePolarisFunction
    {
        private readonly IPipelineClient _pipelineClient;
        private readonly ILogger<PolarisPipelineGetDocument> _logger;

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
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "urns/{urn}/cases/{caseId}/documents/{id:guid}")] HttpRequest req, string urn, int caseId, Guid id)
        {
            Guid currentCorrelationId = default;
            const string loggingName = $"{nameof(PolarisPipelineGetDocument)} - Run";

            try
            {
                var validationResult = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                if (validationResult.InvalidResponseResult != null)
                    return validationResult.InvalidResponseResult;

                currentCorrelationId = validationResult.CurrentCorrelationId;
                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                if (string.IsNullOrWhiteSpace(urn))
                    return BadRequestErrorResponse("Urn is not supplied.", currentCorrelationId, loggingName);

                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Getting document for urn {urn}, caseId {caseId}, id {id}");
                var blobStream = await _pipelineClient.GetDocumentAsync(urn, caseId, id, currentCorrelationId);

                return blobStream != null
                                    ? new OkObjectResult(blobStream)
                                    : NotFoundErrorResponse($"No document found for document id '{id}'.", currentCorrelationId, loggingName);
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

