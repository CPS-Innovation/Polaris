using Common.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Handlers;

namespace PolarisGateway.Functions
{
    public class PolarisPipelineCaseSearchIndexCount
    {
        private readonly ILogger<PolarisPipelineCaseSearchIndexCount> _logger;
        private readonly ICoordinatorClient _coordinatorClient;
        private readonly IInitializationHandler _initializationHandler;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;

        public PolarisPipelineCaseSearchIndexCount(
            ILogger<PolarisPipelineCaseSearchIndexCount> logger,
            ICoordinatorClient coordinatorClient,
            IInitializationHandler initializationHandler,
            IUnhandledExceptionHandler unhandledExceptionHandler)
        {
            _logger = logger;
            _coordinatorClient = coordinatorClient;
            _initializationHandler = initializationHandler;
            _unhandledExceptionHandler = unhandledExceptionHandler;
        }

        [FunctionName(nameof(PolarisPipelineCaseSearchIndexCount))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseSearchCount)] HttpRequest req, string caseUrn, int caseId)
        {
            (Guid CorrelationId, string CmsAuthValues) context = default;
            try
            {
                context = await _initializationHandler.Initialize(req);
                return await _coordinatorClient.GetCaseSearchIndexCount(
                    caseUrn,
                    caseId,
                    context.CorrelationId);
            }
            catch (Exception ex)
            {
                return _unhandledExceptionHandler.HandleUnhandledException(
                  _logger,
                  nameof(PolarisPipelineCaseSearchIndexCount),
                  context.CorrelationId,
                  ex
                );
            }
        }
    }
}

