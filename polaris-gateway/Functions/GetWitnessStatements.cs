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
    public class GetWitnessStatements
    {
        private readonly ILogger<GetWitnessStatements> _logger;
        private readonly ICoordinatorClient _coordinatorClient;
        private readonly IInitializationHandler _initializationHandler;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;

        public GetWitnessStatements(ILogger<GetWitnessStatements> logger,
                                    ICoordinatorClient coordinatorClient,
                                    IInitializationHandler initializationHandler,
                                    IUnhandledExceptionHandler unhandledExceptionHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _coordinatorClient = coordinatorClient ?? throw new ArgumentNullException(nameof(coordinatorClient));
            _initializationHandler = initializationHandler ?? throw new ArgumentNullException(nameof(initializationHandler));
            _unhandledExceptionHandler = unhandledExceptionHandler ?? throw new ArgumentNullException(nameof(unhandledExceptionHandler));
        }

        [FunctionName(nameof(GetWitnessStatements))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.WitnessStatements)] HttpRequest req, string caseUrn, int caseId, int witnessId)
        {
            (Guid CorrelationId, string CmsAuthValues) context = default;
            try
            {
                context = await _initializationHandler.Initialize(req);
                return await _coordinatorClient.GetWitnessStatementsAsync(caseUrn, caseId, witnessId, context.CmsAuthValues, context.CorrelationId);
            }
            catch (Exception ex)
            {
                return _unhandledExceptionHandler.HandleUnhandledException(
                  _logger,
                  nameof(GetWitnessStatements),
                  context.CorrelationId,
                  ex
                );
            }
        }
    }
}