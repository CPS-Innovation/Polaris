using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PolarisGateway.Domain.Logging;
using PolarisGateway.Domain.Validators;
using PolarisGateway.Clients.PolarisPipeline;
using System.Net.Http;
using PolarisGateway.Wrappers;

namespace PolarisGateway.Functions.PolarisPipeline
{
    public class PolarisPipelineGetDocumentSasUrl : BasePolarisFunction
    {
        private readonly IPipelineClient _pipelineClient;
        private readonly ILogger<PolarisPipelineGetDocumentSasUrl> _logger;

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
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "urns/{urn}/cases/{caseId}/documents/{id:guid}/sasUrl")]
            HttpRequest req, string urn, int caseId, Guid id)
        {
            Guid currentCorrelationId = default;
            const string loggingName = $"{nameof(PolarisPipelineGetDocumentSasUrl)} - Run";

            try
            {
                var validationResult = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                if (validationResult.InvalidResponseResult != null)
                    return validationResult.InvalidResponseResult;

                currentCorrelationId = validationResult.CurrentCorrelationId;
                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                if (string.IsNullOrWhiteSpace(urn))
                    return BadRequestErrorResponse("Urn is not supplied.", currentCorrelationId, loggingName);

                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Generating document SAS Url for urn {urn}, caseId {caseId}, id {id}");
                var sasUrl = await _pipelineClient.GenerateDocumentSasUrlAsync(urn, caseId, id, currentCorrelationId);

                return !string.IsNullOrWhiteSpace(sasUrl)
                    ? new OkObjectResult(sasUrl)
                    : NotFoundErrorResponse($"No document SAS URL found for urn {urn}, caseId {caseId}, id {id}\".", currentCorrelationId, loggingName);
            }
            catch (Exception exception)
            {
                return exception switch
                {
                    HttpRequestException => InternalServerErrorResponse(exception, $"A pipeline client http exception occurred when calling {nameof(_pipelineClient.GenerateDocumentSasUrlAsync)}.", currentCorrelationId, loggingName),
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
