using Common.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ddei.Factories;
using PolarisGateway.Services.DdeiOrchestration;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using System;
using Common.Telemetry;

namespace PolarisGateway.Functions;

public class GetDocumentList : BaseFunction
{
    private readonly ILogger<GetDocumentList> _logger;
    private readonly IDdeiCaseDocumentsOrchestrationService _ddeiOrchestrationService;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly ITelemetryClient _telemetryClient;

    public GetDocumentList(
        ILogger<GetDocumentList> logger,
        IDdeiCaseDocumentsOrchestrationService ddeiOrchestrationService,
        IDdeiArgFactory ddeiArgFactory,
        ITelemetryClient telemetryClient)
        : base()
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ddeiOrchestrationService = ddeiOrchestrationService ?? throw new ArgumentNullException(nameof(ddeiOrchestrationService));
        _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
        _telemetryClient = telemetryClient;
    }

    [Function(nameof(GetDocumentList))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Documents)] HttpRequest req, string caseUrn, int caseId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var arg = _ddeiArgFactory.CreateCaseIdentifiersArg(cmsAuthValues, correlationId, caseUrn, caseId);
        var result = await _ddeiOrchestrationService.GetCaseDocuments(arg);

        return new OkObjectResult(result);
    }
}