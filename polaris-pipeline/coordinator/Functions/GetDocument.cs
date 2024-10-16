using System;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Services.BlobStorageService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Extensions;
using Microsoft.AspNetCore.Http;
using coordinator.Helpers;

namespace coordinator.Functions
{
    public class GetDocument : BaseClient
    {
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly ILogger<GetDocument> _logger;
        private const string PdfContentType = "application/pdf";

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
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Document)] HttpRequest req,
            string caseUrn,
            int caseId,
            string documentId,
            [DurableClient] IDurableEntityClient client)
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();
                var response = await GetTrackerDocument(client, caseId, documentId, _logger, currentCorrelationId, nameof(GetDocument));
                var blobName = response.GetBlobName();

                var blobStream = await _blobStorageService.GetBlobOrThrowAsync(blobName);
                return new FileStreamResult(blobStream, PdfContentType);
            }
            catch (Exception ex)
            {
                return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(GetDocument), currentCorrelationId, ex);
            }
        }
    }
}
