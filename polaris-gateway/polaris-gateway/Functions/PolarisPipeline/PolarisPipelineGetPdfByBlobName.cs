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
    public class PolarisPipelineGetPdfByBlobName : BasePolarisFunction
    {
        private readonly IBlobStorageClient _blobStorageClient;
        private readonly ILogger<PolarisPipelineGetPdfByBlobName> _logger;

        public PolarisPipelineGetPdfByBlobName(IBlobStorageClient blobStorageClient, ILogger<PolarisPipelineGetPdfByBlobName> logger, IAuthorizationValidator tokenValidator)
        : base(logger, tokenValidator)
        {
            _blobStorageClient = blobStorageClient;
            _logger = logger;
        }

        [FunctionName(nameof(PolarisPipelineGetPdfByBlobName))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "pdfs/blob/{*blobName}")] HttpRequest req, string blobName)
        {
            Guid currentCorrelationId = default;
            const string loggingName = $"{nameof(PolarisPipelineGetPdfByBlobName)} - Run";

            try
            {
                var validationResult = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                if (validationResult.InvalidResponseResult != null)
                    return validationResult.InvalidResponseResult;
                
                currentCorrelationId = validationResult.CurrentCorrelationId;
                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                if (string.IsNullOrWhiteSpace(blobName))
                    return BadRequestErrorResponse("Blob name is not supplied.", currentCorrelationId, loggingName);

                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Getting PDF document from Polaris blob storage for blob named '{blobName}'");
                var blobStream = await _blobStorageClient.GetDocumentAsync(blobName, currentCorrelationId);

                return blobStream != null
                    ? new OkObjectResult(blobStream)
                    : NotFoundErrorResponse($"No pdf document found for blob name '{blobName}'.", currentCorrelationId, loggingName);
            }
            catch (Exception exception)
            {
                return exception switch
                {
                    RequestFailedException => InternalServerErrorResponse(exception, "A blob storage exception occurred.", currentCorrelationId, loggingName),
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

