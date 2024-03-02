using System;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Logging;
using Common.ValueObjects;
using Ddei.Factories;
using DdeiClient.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Extensions;

namespace coordinator.Functions
{
    public class CancelCheckoutDocument : BaseClient
    {
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;

        public CancelCheckoutDocument(IDdeiClient ddeiClient, IDdeiArgFactory ddeiArgFactory)
        {
            _ddeiClient = ddeiClient;
            _ddeiArgFactory = ddeiArgFactory;
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
                currentCorrelationId = req.Headers.GetCorrelationId();
                var cmsAuthValues = req.Headers.GetCmsAuthValues();

                var response = await GetTrackerDocument(req, client, nameof(CancelCheckoutDocument), caseId, new PolarisDocumentId(polarisDocumentId), log);
                var document = response.CmsDocument;

                var arg = _ddeiArgFactory.CreateDocumentArgDto(

                    cmsAuthValues: cmsAuthValues,
                    correlationId: currentCorrelationId,
                    urn: caseUrn,
                    caseId: int.Parse(caseId),
                    documentCategory: document.CmsDocType.DocumentCategory,
                    documentId: int.Parse(document.CmsDocumentId),
                    versionId: document.CmsVersionId
                );
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
