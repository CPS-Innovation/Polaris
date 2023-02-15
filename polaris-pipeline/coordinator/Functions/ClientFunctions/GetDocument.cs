using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Common.Constants;
using Common.Logging;
using coordinator.Clients;
using coordinator.Domain.Tracker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.ClientFunctions
{
    public class GetDocument
    {
        private readonly IBlobStorageClient _blobStorageClient;

        public GetDocument(IBlobStorageClient blobStorageClient)
        {
            _blobStorageClient = blobStorageClient;
        }

        [FunctionName(nameof(GetDocument))]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "cases/{caseUrn}/{caseId}/documents/{documentId}")] HttpRequestMessage req,
            string caseUrn,
            string caseId,
            Guid documentId, 
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            const string loggingName = $"{nameof(GetDocument)} - {nameof(HttpStart)}";
            const string correlationErrorMessage = "Invalid correlationId. A valid GUID is required.";

            req.Headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIdValues);
            if (correlationIdValues == null)
            {
                log.LogMethodFlow(Guid.Empty, loggingName, correlationErrorMessage);
                return new BadRequestObjectResult(correlationErrorMessage);
            }

            var correlationId = correlationIdValues.FirstOrDefault();
            if (!Guid.TryParse(correlationId, out var currentCorrelationId))
                if (currentCorrelationId == Guid.Empty)
                {
                    log.LogMethodFlow(Guid.Empty, loggingName, correlationErrorMessage);
                    return new BadRequestObjectResult(correlationErrorMessage);
                }

            log.LogMethodEntry(currentCorrelationId, loggingName, caseId);

            var entityId = new EntityId(nameof(Domain.Tracker), caseId);
            var stateResponse = await client.ReadEntityStateAsync<Domain.Tracker.Tracker>(entityId);
            if (!stateResponse.EntityExists)
            {
                var baseMessage = $"No pipeline tracker found with id '{caseId}'";
                log.LogMethodFlow(currentCorrelationId, loggingName, baseMessage);
                return new NotFoundObjectResult(baseMessage);
            }

            var document = stateResponse.EntityState.Documents.GetDocument(documentId); 
            if (document == null)
            {
                var baseMessage = $"No document found with id '{documentId}'";
                log.LogMethodFlow(currentCorrelationId, loggingName, baseMessage);
                return new NotFoundObjectResult(baseMessage);
            }

            var blobName = document.PdfBlobName;
            log.LogMethodFlow(currentCorrelationId, loggingName, $"Getting PDF document from Polaris blob storage for blob named '{blobName}'");
            var blobStream = await _blobStorageClient.GetDocumentAsync(blobName, currentCorrelationId);

            return blobStream != null
                ? new OkObjectResult(blobStream)
                : null; 
        }
    }
}
