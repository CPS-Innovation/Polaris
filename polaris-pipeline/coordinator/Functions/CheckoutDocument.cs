using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Logging;
using DdeiClient.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Extensions;
using coordinator.Durable.Entity;
using Ddei.Factories.Contracts;

namespace coordinator.Functions
{
    public class CheckoutDocument
    {
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;

        public CheckoutDocument(IDdeiClient ddeiClient, IDdeiArgFactory ddeiArgFactory)
        {
            _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
            _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
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
                currentCorrelationId = req.Headers.GetCorrelationId();
                var cmsAuthValues = req.Headers.GetCmsAuthValues();

                var entity = await CaseDurableEntity.GetReadOnlyEntityState(client, caseId);
                var document = entity.CmsDocuments.First(doc => doc.PolarisDocumentId.ToString() == polarisDocumentId);

                var arg = _ddeiArgFactory.CreateDocumentArgDto(
                    cmsAuthValues: cmsAuthValues,
                    correlationId: currentCorrelationId,
                    urn: caseUrn,
                    caseId: int.Parse(caseId),
                    documentCategory: document.CmsDocType.DocumentCategory,
                    documentId: int.Parse(document.CmsDocumentId),
                    versionId: document.CmsVersionId
                );
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
