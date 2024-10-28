using Common.Configuration;
using Ddei;
using Ddei.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PolarisGateway.Handlers;

namespace PolarisGateway.Functions
{
    public class GetWitnesses
    {
        private readonly ILogger<GetWitnesses> _logger;
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;
        private readonly IInitializationHandler _initializationHandler;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;

        public GetWitnesses(ILogger<GetWitnesses> logger,
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

        [FunctionName(nameof(GetWitnesses))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseWitnesses)] HttpRequest req, string caseUrn, int caseId)
        {
            (Guid CorrelationId, string CmsAuthValues) context = default;

            try
            {
                context = await _initializationHandler.Initialize(req);
                var arg = _ddeiArgFactory.CreateCaseIdentifiersArg(context.CmsAuthValues, context.CorrelationId, caseUrn, caseId);
                var result = await _ddeiClient.GetWitnessesAsync(arg);

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                return _unhandledExceptionHandler.HandleUnhandledExceptionActionResult(
                      _logger,
                      nameof(GetWitnesses),
                      context.CorrelationId,
                      ex
                    );
            }
        }
    }
}