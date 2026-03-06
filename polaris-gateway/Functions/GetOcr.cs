using Common.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PolarisGateway.Services.Artefact;
using PolarisGateway.Services.Artefact.Domain;
using System;
using System.Net;
using System.Threading.Tasks;
using Common.Extensions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Common.Domain.Ocr;


namespace PolarisGateway.Functions;

public class GetOcr : BaseFunction
{
    private const string tokenQueryParamName = "token";
    private const string isOcrProcessedParamName = "isOcrProcessed";
    private const string ForceRefreshParamName = "ForceRefresh";
    private readonly ILogger<GetOcr> _logger;
    private readonly IOcrArtefactService _ocrArtefactService;

    public GetOcr(
        ILogger<GetOcr> logger,
        IOcrArtefactService ocrArtefactService)
        : base()
    {
        _logger = logger.ExceptionIfNull();
        _ocrArtefactService = ocrArtefactService.ExceptionIfNull();
    }

    [Function(nameof(GetOcr))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(GetOcr), tags: ["Documents"], Summary = "Artefact Result", Description = "Gives the artefact results")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter(name: "caseUrn", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Case URN", Description = "The URN identifier of the case")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("documentId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the document", Required = true)]
    [OpenApiParameter("versionId", In = ParameterLocation.Path, Type = typeof(long), Description = "The version Id of the document", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ArtefactResult<AnalyzeResults>), Summary = "Artefact Result", Description = "Gives the artefact results")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Ocr)] HttpRequest req, string caseUrn, int caseId, string documentId, long versionId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var isOcrProcessed = req.Query.ContainsKey(isOcrProcessedParamName) && bool.Parse(req.Query[isOcrProcessedParamName]);
        var forceRefresh = req.Query.ContainsKey(ForceRefreshParamName) && bool.Parse(req.Query[ForceRefreshParamName]);
        var token = req.Query.ContainsKey(tokenQueryParamName) ?
            Guid.Parse(req.Query[tokenQueryParamName]) :
            (Guid?)null;

        var ocrResult = await _ocrArtefactService.GetOcrAsync(cmsAuthValues, correlationId, caseUrn, caseId, documentId, versionId, isOcrProcessed, token, forceRefresh);
        return ocrResult.Status switch
        {
            ResultStatus.ArtefactAvailable => new JsonResult(ocrResult.Artefact)
            {
                StatusCode = (int)HttpStatusCode.OK
            },
            ResultStatus.PollWithToken => new JsonResult(new
            {
                NextUrl = $"{req.GetDisplayUrl()}{(req.QueryString.Value.StartsWith('?') ? "&" : "?")}{tokenQueryParamName}={ocrResult.ContinuationToken}"
            })
            {
                StatusCode = (int)HttpStatusCode.Accepted // the client will understand 202 as a signal to poll again
            },
            ResultStatus.Failed => new JsonResult(ocrResult)
            {
                StatusCode = (int)HttpStatusCode.UnsupportedMediaType
            },
            _ => new JsonResult(ocrResult) { StatusCode = (int)HttpStatusCode.InternalServerError },
        };
    }
}