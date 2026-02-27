using Common.Configuration;
using Common.Dto.Response.Documents;
using Common.Telemetry;
using Ddei.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PolarisGateway.Services.DdeiOrchestration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
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
    [OpenApiOperation(operationId: nameof(GetDocumentList), tags: ["Documents"], Summary = "Get Document List", Description = "Getting the list of documents")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter(name: "caseUrn", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Case URN", Description = "The URN identifier of the case")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<DocumentDto>), Summary = "Document List", Description = "Returns list of documennts")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Documents)] HttpRequest req, string caseUrn, int caseId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var arg = _mdsArgFactory.CreateCaseIdentifiersArg(cmsAuthValues, correlationId, caseUrn, caseId);
        var result = await _mdsOrchestrationService.GetCaseDocuments(arg);

        return new OkObjectResult(result);
    }
}