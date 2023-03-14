using System;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Logging;
using Ddei.Domain.CaseData.Args;
using Ddei.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.ClientFunctions
{
    public class CheckoutDocument : BaseClientFunction
    {
        private readonly IDocumentService _documentService;

        public CheckoutDocument(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        const string loggingName = $"{nameof(GetDocument)} - {nameof(HttpStart)}";

        [FunctionName(nameof(CheckoutDocument))]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = RestApi.DocumentCheckout)] HttpRequestMessage req,
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

                CmsDocumentArg arg = new CmsDocumentArg
                {
                    Urn = caseUrn,
                    CaseId = long.Parse(caseId),
                    CmsDocCategory = document.CmsDocType.DocumentCategory,
                    DocumentId = int.Parse(document.CmsDocumentId),
                };
                await _documentService.CheckoutDocument(arg);

                return new OkResult();
            }
            catch (Exception ex)
            {
                log.LogMethodError(currentCorrelationId, loggingName, ex.Message, ex);
                return new StatusCodeResult(500);
            }

        }
    }
}
