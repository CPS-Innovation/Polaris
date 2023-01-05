using System;
using System.Threading.Tasks;
using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RumpoleGateway.Domain.Logging;
using RumpoleGateway.Services;
using RumpoleGateway.Domain.Validators;

namespace RumpoleGateway.Functions.RumpolePipeline
{
    public class RumpolePipelineGetSasUrl : BaseRumpoleFunction
    {
        private readonly ISasGeneratorService _sasGeneratorService;
        private readonly ILogger<RumpolePipelineGetSasUrl> _logger;

        public RumpolePipelineGetSasUrl(IAuthorizationValidator tokenValidator, ILogger<RumpolePipelineGetSasUrl> logger, ISasGeneratorService sasGeneratorService)
            : base(logger, tokenValidator)
        {
            _sasGeneratorService = sasGeneratorService ?? throw new ArgumentNullException(nameof(sasGeneratorService));
            _logger = logger;
        }

        [FunctionName("RumpolePipelineGetSasUrl")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "pdf/sasUrl/{*blobName}")]
            HttpRequest req, string blobName)
        {
            Guid currentCorrelationId = default;
            const string loggingName = "RumpolePipelineGetSasUrl - Run";

            try
            {
                var validationResult = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                if (validationResult.InvalidResponseResult != null)
                    return validationResult.InvalidResponseResult;
                
                currentCorrelationId = validationResult.CurrentCorrelationId;
                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);
                
                if (string.IsNullOrWhiteSpace(blobName))
                    return BadRequestErrorResponse("Blob name is not supplied.", currentCorrelationId, loggingName);

                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Generating SAS Url for '{blobName}'");
                var sasUrl = await _sasGeneratorService.GenerateSasUrlAsync(blobName, currentCorrelationId);

                return !string.IsNullOrWhiteSpace(sasUrl)
                    ? new OkObjectResult(sasUrl)
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
