using Common.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ddei.Factories;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using System;
using Common.Telemetry;
using PolarisGateway.Services.MdsOrchestration;

namespace PolarisGateway.Functions;

public class GetDocumentList : BaseFunction
{
    private readonly ILogger<GetDocumentList> _logger;
    private readonly IMdsCaseDocumentsOrchestrationService _mdsOrchestrationService;
    private readonly IMdsArgFactory _mdsArgFactory;
    private readonly ITelemetryClient _telemetryClient;

    public GetDocumentList(
        ILogger<GetDocumentList> logger,
        IMdsCaseDocumentsOrchestrationService mdsOrchestrationService,
        IMdsArgFactory mdsArgFactory,
        ITelemetryClient telemetryClient)
        : base()
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mdsOrchestrationService = mdsOrchestrationService ?? throw new ArgumentNullException(nameof(mdsOrchestrationService));
        _mdsArgFactory = mdsArgFactory ?? throw new ArgumentNullException(nameof(mdsArgFactory));
        _telemetryClient = telemetryClient;
    }

    [Function(nameof(GetDocumentList))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Documents)] HttpRequest req, string caseUrn, int caseId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var arg = _mdsArgFactory.CreateCaseIdentifiersArg(cmsAuthValues, correlationId, caseUrn, caseId);
        var result = await _mdsOrchestrationService.GetCaseDocuments(arg);

        return new OkObjectResult(result);
    }
}