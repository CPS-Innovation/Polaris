using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Services.SasGeneratorService;
using Common.Constants;
using Common.Logging;
using coordinator.Domain.Tracker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Configuration;

namespace coordinator.Functions.ClientFunctions
{
    public class GetDocumentSasUrl
    {
        private readonly ISasGeneratorService _sasGeneratorService;

        public GetDocumentSasUrl(ISasGeneratorService sasGeneratorService)
        {
            _sasGeneratorService = sasGeneratorService;
        }

        [FunctionName(nameof(GetDocumentSasUrl))]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = RestApi.DocumentSasUrl)] HttpRequestMessage req,
            string caseUrn,
            string caseId,
            Guid documentId, 
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            const string loggingName = $"{nameof(GetDocumentSasUrl)} - {nameof(HttpStart)}";
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
            var stateResponse = await client.ReadEntityStateAsync<Tracker>(entityId);
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
            var sasUrl = await _sasGeneratorService.GenerateSasUrlAsync(blobName, currentCorrelationId);

            return !string.IsNullOrEmpty(sasUrl) ? new OkObjectResult(sasUrl) : null; 
        }
    }
}
