using System;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Logging;
using Common.Services.BlobStorageService.Contracts;
using Common.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.DurableEntity
{
    public class GetDocumentClient : BaseClient
    {
        private readonly IPolarisBlobStorageService _blobStorageService;

        public GetDocumentClient(IPolarisBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }

        [FunctionName(nameof(GetDocumentClient))]
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
                var polarisDocumentIdValue = new PolarisDocumentId(polarisDocumentId);
                var response = await GetTrackerDocument(req, client, caseId, polarisDocumentIdValue);

                if (!response.Success)
                    return response.Error;

                currentCorrelationId = response.CorrelationId;

                var blobName = response.GetBlobName();
                var blobStream = await _blobStorageService.GetDocumentAsync(blobName, currentCorrelationId);

                return blobStream != null
                    ? new OkObjectResult(blobStream)
                    : null;
            }
            catch (Exception ex)
            {
                log.LogMethodError(currentCorrelationId, nameof(GetDocumentClient), ex.Message, ex);
                return new StatusCodeResult(500);
            }
        }
    }
}
