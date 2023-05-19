using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Constants;
using Common.Logging;
using Ddei.Domain.CaseData.Args;
using DdeiClient.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.DurableEntity.Client.Document
{
    public class CancelCheckoutDocumentClient : BaseClient
    {
        private readonly IDdeiClient _gatewayDdeiService;

        public CancelCheckoutDocumentClient(IDdeiClient gatewayDdeiService)
        {
            _gatewayDdeiService = gatewayDdeiService;
        }

        const string loggingName = $"{nameof(CancelCheckoutDocumentClient)} - {nameof(HttpStart)}";

        [FunctionName(nameof(CancelCheckoutDocumentClient))]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = RestApi.DocumentCheckout)] HttpRequestMessage req,
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

                var docType = response.CmsDocument.CmsDocType.DocumentType;
                if (docType == "PCD" || docType == "DAC")
                {
                    return new BadRequestObjectResult($"Invalid document type specified : {docType}");
                }

                currentCorrelationId = response.CorrelationId;
                var document = response.CmsDocument;
                var blobName = document.PdfBlobName;

                log.LogMethodFlow(currentCorrelationId, loggingName, $"Cancel checkout document for caseId: {caseId}, documentId: {documentId}");

                var cmsAuthValues = req.Headers.GetValues(HttpHeaderKeys.CmsAuthValues).FirstOrDefault();
                if (string.IsNullOrEmpty(cmsAuthValues))
                {
                    log.LogMethodFlow(currentCorrelationId, loggingName, $"No authentication header values specified");
                    throw new ArgumentException(HttpHeaderKeys.CmsAuthValues);
                }

                DdeiCmsDocumentArgDto arg = new DdeiCmsDocumentArgDto
                {
                    CmsAuthValues = cmsAuthValues,
                    CorrelationId = currentCorrelationId,
                    Urn = caseUrn,
                    CaseId = long.Parse(caseId),
                    CmsDocCategory = document.CmsDocType.DocumentCategory,
                    DocumentId = int.Parse(document.CmsDocumentId),
                    VersionId = document.CmsVersionId
                };
                await _gatewayDdeiService.CancelCheckoutDocument(arg);

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
