using Common.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Ddei;
using PolarisGateway.Handlers;
using Ddei.Factories;
using Common.Services.DocumentToggle;
using PolarisGateway.Services.DdeiOrchestration;

namespace PolarisGateway.Functions
{
    public class GetDocumentList
    {
        private readonly ILogger<GetDocumentList> _logger;
        private readonly IDdeiOrchestrationService _ddeiOrchestrationService;
        private readonly IDdeiArgFactory _ddeiArgFactory;

        private readonly IInitializationHandler _initializationHandler;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;

        public GetDocumentList(
            ILogger<GetDocumentList> logger,
            IDdeiOrchestrationService ddeiOrchestrationService,
            IDdeiArgFactory ddeiArgFactory,
            IInitializationHandler initializationHandler,
            IUnhandledExceptionHandler unhandledExceptionHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ddeiOrchestrationService = ddeiOrchestrationService ?? throw new ArgumentNullException(nameof(ddeiOrchestrationService));
            _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
            _initializationHandler = initializationHandler ?? throw new ArgumentNullException(nameof(initializationHandler));
            _unhandledExceptionHandler = unhandledExceptionHandler ?? throw new ArgumentNullException(nameof(unhandledExceptionHandler));
        }

        [FunctionName(nameof(GetDocumentList))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Documents)] HttpRequest req, string caseUrn, int caseId)
        {
            (Guid CorrelationId, string CmsAuthValues) context = default;
            try
            {
                context = await _initializationHandler.Initialize(req);
                var arg = _ddeiArgFactory.CreateCaseIdentifiersArg(context.CmsAuthValues, context.CorrelationId, caseUrn, caseId);
                var result = await _ddeiOrchestrationService.GetCaseDocuments(arg);

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                return _unhandledExceptionHandler.HandleUnhandledExceptionActionResult(
                      _logger,
                      nameof(GetDocumentList),
                      context.CorrelationId,
                      ex
                    );
            }
        }
    }
}

