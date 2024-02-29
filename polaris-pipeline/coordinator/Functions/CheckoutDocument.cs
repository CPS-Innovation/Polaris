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
    public class CheckoutDocument : BaseClient
    {
        private readonly IDdeiClient _ddeiClient;

        public CheckoutDocument(IDdeiClient ddeiClient)
        {
            _ddeiClient = ddeiClient;
        }

        [FunctionName(nameof(CheckoutDocument))]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = RestApi.DocumentCheckout)] HttpRequestMessage req,
            string caseUrn,
            string caseId,
            string polarisDocumentId,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            Guid currentCorrelationId = default;

            try
            {
                var cmsAuthValues = req.Headers.GetValues(HttpHeaderKeys.CmsAuthValues).FirstOrDefault();
                if (string.IsNullOrEmpty(cmsAuthValues))
                {
                    throw new ArgumentException(HttpHeaderKeys.CmsAuthValues);
                }

                var response = await GetTrackerDocument(req, client, nameof(CheckoutDocument), caseId, new PolarisDocumentId(polarisDocumentId), log);

                if (!response.Success)
                    return response.Error;

                var docType = response.CmsDocument.CmsDocType.DocumentType;
                if (docType == "PCD" || docType == "DAC")
                {
                    return new BadRequestObjectResult($"Invalid document type specified : {docType}");
                }

                currentCorrelationId = response.CorrelationId;
                var document = response.CmsDocument;

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
                var result = await _ddeiClient.CheckoutDocumentAsync(arg);

                return result.IsSuccess
                    ? new OkResult()
                    : new ConflictObjectResult(result.LockingUserName);

            }
            catch (Exception ex)
            {
                log.LogMethodError(currentCorrelationId, nameof(CheckoutDocument), ex.Message, ex);
                return new StatusCodeResult(500);
            }
        }
    }
}
