using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Constants;
using Common.Logging;
using coordinator.Domain.Tracker;
using Ddei.Domain.CaseData.Args;
using Ddei.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.ClientFunctions
{
    public class CancelCheckoutDocument
    {
        private readonly IDocumentService _documentService;

        public CancelCheckoutDocument(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        const string loggingName = $"{nameof(GetDocument)} - {nameof(HttpStart)}";
        const string correlationErrorMessage = "Invalid correlationId. A valid GUID is required.";

        [FunctionName(nameof(CancelCheckoutDocument))]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = RestApi.DocumentCheckout)] HttpRequestMessage req,
            string caseUrn,
            string caseId,
            Guid documentId,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
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

            log.LogMethodFlow(currentCorrelationId, loggingName, $"Cancel checkout document for caseId: {caseId}, documentId: {documentId}");

            CmsDocumentArg arg = new CmsDocumentArg
            {
                Urn = caseUrn,
                CaseId = long.Parse(caseId),
                CmsDocCategory = document.CmsDocType.DocumentCategory,
                DocumentId = int.Parse(document.CmsDocumentId),
            };
            await _documentService.CancelCheckoutDocument(arg);

            return new OkResult();
        }
    }
}
