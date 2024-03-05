using System;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Extensions;
using Common.ValueObjects;
using coordinator.Helpers;
using Ddei.Factories;
using DdeiClient.Services;
using Microsoft.AspNetCore.Http;
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
        private readonly IDdeiArgFactory _ddeiArgFactory;
        private readonly ILogger<CheckoutDocument> _logger;

        public CheckoutDocument(IDdeiClient ddeiClient, IDdeiArgFactory ddeiArgFactory, ILogger<CheckoutDocument> logger)
        {
            _ddeiClient = ddeiClient;
            _ddeiArgFactory = ddeiArgFactory;
            _logger = logger;
        }

        [FunctionName(nameof(CheckoutDocument))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = RestApi.DocumentCheckout)] HttpRequest req,
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

                var result = await _ddeiClient.CheckoutDocumentAsync(arg);

                return result.IsSuccess
                    ? new OkResult()
                    : new ConflictObjectResult(result.LockingUserName);

            }
            catch (Exception ex)
            {
                return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(CheckoutDocument), currentCorrelationId, ex);
            }
        }
    }
}
