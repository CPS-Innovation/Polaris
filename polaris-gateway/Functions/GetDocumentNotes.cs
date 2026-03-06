using Common.Configuration;
using Common.Dto.Response.Document;
using Common.Extensions;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class GetDocumentNotes : BaseFunction
{
    private readonly ILogger<GetDocumentNotes> _logger;
    private readonly IMdsClient _mdsClient;
    private readonly IMdsArgFactory _mdsArgFactory;

    public GetDocumentNotes(ILogger<GetDocumentNotes> logger,
        IMdsClient mdsClient,
        IMdsArgFactory mdsArgFactory)
        : base()
    {
        _logger = logger.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
        _mdsArgFactory = mdsArgFactory.ExceptionIfNull();
    }

    [Function(nameof(GetDocumentNotes))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(GetDocumentNotes), tags: ["Documents"], Summary = "Get Document Note List", Description = "Getting the list of document notes")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter(name: "caseUrn", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Case URN", Description = "The URN identifier of the case")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("documentId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the document", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<DocumentNoteDto>), Summary = "Document Note List", Description = "Returns list of document notes")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.DocumentNotes)] HttpRequest req, string caseUrn, int caseId, string documentId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var arg = _mdsArgFactory.CreateDocumentArgDto(cmsAuthValues, correlationId, caseUrn, caseId, documentId);

        var result = await _mdsClient.GetDocumentNotesAsync(arg);

        return new OkObjectResult(result);
    }
}