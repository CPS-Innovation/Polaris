using System;
using System.Threading.Tasks;
using Common.Configuration;
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
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = RestApi.DocumentCheckout)] HttpRequest req,
            string caseUrn,
            string caseId,
            string documentId,
            [DurableClient] IDurableEntityClient client)
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();
                var cmsAuthValues = req.Headers.GetCmsAuthValues();

                var response = await GetTrackerDocument(client, caseId, documentId, _logger, currentCorrelationId, nameof(CancelCheckoutDocument));
                var document = response.CmsDocument;

                var arg = _ddeiArgFactory.CreateDocumentArgDto(

                    cmsAuthValues: cmsAuthValues,
                    correlationId: currentCorrelationId,
                    urn: caseUrn,
                    caseId: int.Parse(caseId),
                    documentId: int.Parse(document.CmsDocumentId),
                    versionId: document.VersionId
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
