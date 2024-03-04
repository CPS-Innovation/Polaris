using System;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Services.BlobStorageService.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Extensions;
using Common.ValueObjects;
using Microsoft.AspNetCore.Http;
using coordinator.Helpers;

namespace coordinator.Functions
{
    public class GetDocument : BaseClient
    {
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly ILogger<GetDocument> _logger;

        public GetDocument(IPolarisBlobStorageService blobStorageService, ILogger<GetDocument> logger)
        {
            _blobStorageService = blobStorageService;
            _logger = logger;
        }

        [FunctionName(nameof(GetDocument))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Consistent API parameters")]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = RestApi.Document)] HttpRequest req,
            string caseUrn,
            int caseId,
            string polarisDocumentId,
            [DurableClient] IDurableEntityClient client)
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();
                var response = await GetTrackerDocument(client, caseId.ToString(), new PolarisDocumentId(polarisDocumentId));
                var blobName = response.GetBlobName();

                var blobStream = await _blobStorageService.GetDocumentAsync(blobName, currentCorrelationId);
                return new OkObjectResult(blobStream);
            }
            catch (Exception ex)
            {
                return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(GetDocument), currentCorrelationId, ex);
            }
        }
    }
}
