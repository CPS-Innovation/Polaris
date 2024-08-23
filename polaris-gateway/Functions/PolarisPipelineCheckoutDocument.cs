using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using PolarisGateway.Clients.Coordinator;
using Common.ValueObjects;
using Microsoft.Azure.Functions.Worker;
using PolarisGateway.Handlers;

namespace PolarisGateway.Functions
{
    public class PolarisPipelineCheckoutDocument
    {
        private readonly ILogger<PolarisPipelineCheckoutDocument> _logger;
        private readonly ICoordinatorClient _coordinatorClient;
        private readonly IInitializationHandler _initializationHandler;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;

        public PolarisPipelineCheckoutDocument(
            ILogger<PolarisPipelineCheckoutDocument> logger,
            ICoordinatorClient coordinatorClient,
            IInitializationHandler initializationHandler,
            IUnhandledExceptionHandler unhandledExceptionHandler)
        {
            _logger = logger;
            _coordinatorClient = coordinatorClient;
            _initializationHandler = initializationHandler;
            _unhandledExceptionHandler = unhandledExceptionHandler;
        }

        [Function(nameof(PolarisPipelineCheckoutDocument))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.DocumentCheckout)] HttpRequest req, string caseUrn, int caseId, string polarisDocumentId)
        {
            (Guid CorrelationId, string CmsAuthValues) context = default;
            try
            {
                context = await _initializationHandler.Initialize(req);
                return await _coordinatorClient.CheckoutDocumentAsync(
                    caseUrn,
                    caseId,
                    new PolarisDocumentId(polarisDocumentId),
                    context.CmsAuthValues,
                    context.CorrelationId);
            }
            catch (Exception ex)
            {
                return _unhandledExceptionHandler.HandleUnhandledException(
                  _logger,
                  nameof(PolarisPipelineCheckoutDocument),
                  context.CorrelationId,
                  ex
                );
            }
        }
    }
}

