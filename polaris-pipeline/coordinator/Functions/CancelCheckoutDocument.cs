using System;
using System.Threading.Tasks;
using Common.Configuration;
using Common.ValueObjects;
using Ddei.Factories;
using DdeiClient.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Extensions;
using Microsoft.AspNetCore.Http;
using coordinator.Helpers;

namespace coordinator.Functions
{
    public class CancelCheckoutDocument : BaseClient
    {
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;
        private readonly ILogger<CancelCheckoutDocument> _logger;

        public CancelCheckoutDocument(IDdeiClient ddeiClient, IDdeiArgFactory ddeiArgFactory, ILogger<CancelCheckoutDocument> logger)
        {
            _ddeiClient = ddeiClient;
            _ddeiArgFactory = ddeiArgFactory;
            _logger = logger;
        }

        [FunctionName(nameof(CancelCheckoutDocument))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = RestApi.DocumentCheckout)] HttpRequest req,
            string caseUrn,
            string caseId,
            string polarisDocumentId,
            [DurableClient] IDurableEntityClient client)
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();
                var cmsAuthValues = req.Headers.GetCmsAuthValues();

                var response = await GetTrackerDocument(client, caseId, new PolarisDocumentId(polarisDocumentId));
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
                return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(CancelCheckoutDocument), currentCorrelationId, ex);
            }
        }
    }
}
