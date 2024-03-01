using System;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Logging;
using Common.Services.BlobStorageService.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Extensions;
using coordinator.Domain;

namespace coordinator.Functions
{
    public class GetDocument : BaseClient
    {
        private readonly IPolarisBlobStorageService _blobStorageService;

        public GetDocument(IPolarisBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }

        [FunctionName(nameof(GetDocument))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Consistent API parameters")]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = RestApi.Document)] HttpRequestMessage req,
            string caseUrn,
            string caseId,
            string polarisDocumentId,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();

                var blobName = PdfBlobNameHelper.GetPdfBlobName(caseId, polarisDocumentId);
                var blobStream = await _blobStorageService.GetDocumentAsync(blobName, currentCorrelationId);

                return blobStream != null
                    ? new OkObjectResult(blobStream)
                    : new NotFoundObjectResult($"No document blob found with id '{polarisDocumentId}'");
            }
            catch (Exception ex)
            {
                log.LogMethodError(currentCorrelationId, nameof(GetDocument), ex.Message, ex);
                return new StatusCodeResult(500);
            }
        }
    }
}
