using System;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Clients.Contracts;
using Common.Configuration;
using Common.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.ClientFunctions.Document
{
    public class GetDocument : BaseClientFunction
    {
        private readonly IPolarisStorageClient _blobStorageClient;

        const string loggingName = $"{nameof(GetDocument)} - {nameof(HttpStart)}";

        public GetDocument(IPolarisStorageClient blobStorageClient)
        {
            _blobStorageClient = blobStorageClient;
        }

        [FunctionName(nameof(GetDocument))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Consistent API parameters")]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = RestApi.Document)] HttpRequestMessage req,
            string caseUrn,
            string caseId,
            Guid documentId,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            Guid currentCorrelationId = default;

            try
            {
                var response = await GetTrackerDocument(req, client, loggingName, caseId, documentId, log);

                if (!response.Success)
                    return response.Error;

                currentCorrelationId = response.CorrelationId;
                var document = response.Document;
                var blobName = document.PdfBlobName;
                log.LogMethodFlow(currentCorrelationId, loggingName, $"Getting PDF document from Polaris blob storage for blob named '{blobName}'");
                var blobStream = await _blobStorageClient.GetDocumentAsync(blobName, currentCorrelationId);

                return blobStream != null
                    ? new OkObjectResult(blobStream)
                    : null;
            }
            catch (Exception ex)
            {
                log.LogMethodError(currentCorrelationId, loggingName, ex.Message, ex);
                return new StatusCodeResult(500);
            }

        }
    }
}
