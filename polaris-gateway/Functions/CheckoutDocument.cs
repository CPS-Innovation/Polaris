using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using PolarisGateway.Handlers;
using Ddei;
using Ddei.Factories;

namespace PolarisGateway.Functions
{
    public class CheckoutDocument
    {
        private readonly ILogger<CheckoutDocument> _logger;
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;
        private readonly IInitializationHandler _initializationHandler;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;

        public CheckoutDocument(
            ILogger<CheckoutDocument> logger,
            IDdeiClient ddeiClient,
            IDdeiArgFactory ddeiArgFactory,
            IInitializationHandler initializationHandler,
            IUnhandledExceptionHandler unhandledExceptionHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
            _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
            _initializationHandler = initializationHandler ?? throw new ArgumentNullException(nameof(initializationHandler));
            _unhandledExceptionHandler = unhandledExceptionHandler ?? throw new ArgumentNullException(nameof(unhandledExceptionHandler));
        }

        [FunctionName(nameof(CheckoutDocument))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.DocumentCheckout)] HttpRequest req, string caseUrn, int caseId, string documentId, long versionId)
        {
            (Guid CorrelationId, string CmsAuthValues) context = default;
            try
            {
                context = await _initializationHandler.Initialize(req);
                var arg = _ddeiArgFactory.CreateDocumentVersionArgDto(
                         cmsAuthValues: context.CmsAuthValues,
                         correlationId: context.CorrelationId,
                         urn: caseUrn,
                         caseId: caseId,
                         documentId: documentId,
                         versionId: versionId
                 );

                var result = await _ddeiClient.CheckoutDocumentAsync(arg);

                return result.IsSuccess
                    ? new OkResult()
                    : new ConflictObjectResult(result.LockingUserName);
            }
            catch (Exception ex)
            {
                return _unhandledExceptionHandler.HandleUnhandledExceptionActionResult(
                  _logger,
                  nameof(CheckoutDocument),
                  context.CorrelationId,
                  ex
                );
            }
        }
    }
}

