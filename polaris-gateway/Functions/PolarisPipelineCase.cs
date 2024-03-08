using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Handlers;

namespace PolarisGateway.Functions
{
    // note: the analytics KQL queries refer to "PolarisPipelineCase" as the function name,
    //  if we change this then we must change the KQL queries to be `| ... ("PolarisPipelineCase" or "NewName")
    public class PolarisPipelineCase
    {
        private readonly ILogger<PolarisPipelineCase> _logger;
        private readonly ICoordinatorClient _coordinatorClient;
        private readonly IInitializationHandler _initializationHandler;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;

        public PolarisPipelineCase(
            ILogger<PolarisPipelineCase> logger,
            ICoordinatorClient coordinatorClient,
            IInitializationHandler initializationHandler,
            IUnhandledExceptionHandler unhandledExceptionHandler)
        {
            _logger = logger;
            _coordinatorClient = coordinatorClient;
            _initializationHandler = initializationHandler;
            _unhandledExceptionHandler = unhandledExceptionHandler;
        }

        [FunctionName(nameof(PolarisPipelineCase))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.Case)] HttpRequest req, string caseUrn, int caseId)
        {
            (Guid CorrelationId, string CmsAuthValues) context = default;

            try
            {
                context = await _initializationHandler.Initialize(req);
                return await _coordinatorClient.RefreshCaseAsync(caseUrn, caseId, context.CmsAuthValues, context.CorrelationId);
            }
            catch (Exception ex)
            {
                return _unhandledExceptionHandler.HandleUnhandledException(
                      _logger,
                      nameof(PolarisPipelineCase),
                      context.CorrelationId,
                      ex
                    );
            }
        }
    }
}

