using Common.Configuration;
using Common.Domain.Document;
using Common.Extensions;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Domain.Args;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class ToggleIsUnusedDocument : BaseFunction
{
    private readonly ILogger<ToggleIsUnusedDocument> _logger;
    private readonly IMdsClient _mdsClient;
    public ToggleIsUnusedDocument(
        ILogger<ToggleIsUnusedDocument> logger,
        IMdsClient mdsClient)
    {
        _logger = logger.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
    }

    [Function(nameof(ToggleIsUnusedDocument))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(ReclassifyDocument), tags: ["Documents"], Summary = "Toggle Is Unused Document", Description = "Toggle Is Unused Document")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter(name: "caseUrn", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Case URN", Description = "The URN identifier of the case")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("documentId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the document", Required = true)]
    [OpenApiParameter("isUnused", In = ParameterLocation.Path, Type = typeof(string), Description = "Is un used document", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object), Summary = "Document Note List", Description = "Returns list of document notes")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.ToggleIsUnusedDocument)] HttpRequest req,
        string caseUrn,
        int caseId,
        string documentId,
        string isUnused)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var toggleIsUnusedDocumentDto = new MdsToggleIsUnusedDocumentDto
        {
            CaseId = caseId,
            CmsAuthValues = cmsAuthValues,
            CorrelationId = correlationId,
            DocumentId = DocumentNature.ToNumericDocumentId(documentId, DocumentNature.Types.Document),
            IsUnused = isUnused,
            Urn = caseUrn,
        };

        return await _mdsClient.ToggleIsUnusedDocumentAsync(toggleIsUnusedDocumentDto) ? new OkResult() : new BadRequestResult();
    }
}