using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Constants;
using Common.Logging;
using Common.ValueObjects;
using Ddei.Domain.CaseData.Args;
using DdeiClient.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions
{
    public class CancelCheckoutDocument : BaseClient
    {
        private readonly IDdeiClient _ddeiClient;

        public CancelCheckoutDocument(IDdeiClient ddeiClient)
        {
            _ddeiClient = ddeiClient;
        }

        [FunctionName(nameof(CancelCheckoutDocument))]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = RestApi.DocumentCheckout)] HttpRequestMessage req,
            string caseUrn,
            string caseId,
            string polarisDocumentId,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            Guid currentCorrelationId = default;

            try
            {
                var response = await GetTrackerDocument(req, client, nameof(CancelCheckoutDocument), caseId, new PolarisDocumentId(polarisDocumentId), log);

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

                var cmsAuthValues = req.Headers.GetValues(HttpHeaderKeys.CmsAuthValues).FirstOrDefault();
                if (string.IsNullOrEmpty(cmsAuthValues))
                {
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
                await _ddeiClient.CancelCheckoutDocumentAsync(arg);

                return new OkResult();
            }
            catch (Exception ex)
            {
                log.LogMethodError(currentCorrelationId, nameof(CancelCheckoutDocument), ex.Message, ex);
                return new StatusCodeResult(500);
            }
        }
    }
}
