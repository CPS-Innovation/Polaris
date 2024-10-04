using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using Common.Telemetry;
using Microsoft.Azure.Functions.Worker;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Handlers;

namespace PolarisGateway.Functions
{
    public class PolarisPipelineCaseDelete : BaseFunction
    {
        private readonly ILogger<PolarisPipelineCaseDelete> _logger;
        private readonly ICoordinatorClient _coordinatorClient;
        private readonly IInitializationHandler _initializationHandler;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;
        private readonly ITelemetryClient _telemetryClient;

        public PolarisPipelineCaseDelete(
            ILogger<PolarisPipelineCaseDelete> logger,
            ICoordinatorClient coordinatorClient,
            IInitializationHandler initializationHandler,
            IUnhandledExceptionHandler unhandledExceptionHandler,
            ITelemetryClient telemetryClient)
        : base(telemetryClient)
        {
            _logger = logger;
            _coordinatorClient = coordinatorClient;
            _initializationHandler = initializationHandler;
            _unhandledExceptionHandler = unhandledExceptionHandler;
            _telemetryClient = telemetryClient;
        }

        [Function(nameof(PolarisPipelineCaseDelete))]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = RestApi.Case)] HttpRequest req, string caseUrn, int caseId)
        {
            (Guid CorrelationId, string? CmsAuthValues) context = default;

            try
            {
                context = await _initializationHandler.Initialize(req);
                return await _coordinatorClient.DeleteCaseAsync(
                    caseUrn,
                    caseId,
                    context.CmsAuthValues,
                    context.CorrelationId);
            }
            catch (Exception ex)
            {
                return _unhandledExceptionHandler.HandleUnhandledException(
                      _logger,
                      nameof(PolarisPipelineCaseDelete),
                      context.CorrelationId,
                      ex
                    );
            }
        }
    }
}

