using Common.Configuration;
using Common.Domain.Pii;
using Common.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PolarisGateway.Models;
using PolarisGateway.Services.Artefact;
using PolarisGateway.Services.Artefact.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;


namespace PolarisGateway.Functions;

public class GetPii : BaseFunction
{
    private const string tokenQueryParamName = "token";
    private const string isOcrProcessedParamName = "isOcrProcessed";
    private const string ForceRefreshParamName = "ForceRefresh";
    private readonly ILogger<GetPii> _logger;
    private readonly IPiiArtefactService _piiArtefactService;

    public GetPii(
        ILogger<GetPii> logger,
        IPiiArtefactService piiArtefactService)
        : base()
    {
        _logger = logger.ExceptionIfNull();
        _piiArtefactService = piiArtefactService.ExceptionIfNull();
    }

    [Function(nameof(GetPii))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(GetPdf), tags: ["Documents"], Summary = "Get pdf", Description = "Gives the pdf")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter(name: "caseUrn", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Case URN", Description = "The URN identifier of the case")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("documentId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the document", Required = true)]
    [OpenApiParameter("versionId", In = ParameterLocation.Path, Type = typeof(long), Description = "The version Id of the document", Required = true)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<PiiLine>), Description = "OCR processing completed successfully")]
    [OpenApiResponseWithBody(HttpStatusCode.Accepted, "application/json", typeof(OcrPollResponse), Description = "OCR is still processing. Client should poll using the provided NextUrl")]
    [OpenApiResponseWithBody(HttpStatusCode.UnsupportedMediaType, "application/json", typeof(OcrResult), Description = "OCR processing failed")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(OcrResult), Description = "Unexpected server error")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]

    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Pii)] HttpRequest req, string caseUrn, int caseId, string documentId, long versionId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var isOcrProcessed = req.Query.ContainsKey(isOcrProcessedParamName) && bool.Parse(req.Query[isOcrProcessedParamName]);
        var forceRefresh = req.Query.ContainsKey(ForceRefreshParamName) && bool.Parse(req.Query[ForceRefreshParamName]);
        var token = req.Query.ContainsKey(tokenQueryParamName) ?
            Guid.Parse(req.Query[tokenQueryParamName]) :
            (Guid?)null;

        var ocrResult = await _piiArtefactService.GetPiiAsync(cmsAuthValues, correlationId, caseUrn, caseId, documentId, versionId, isOcrProcessed, token, forceRefresh);
        return ocrResult.Status switch
        {
            ResultStatus.ArtefactAvailable => new JsonResult(ocrResult.Artefact),
            ResultStatus.PollWithToken => new JsonResult(new
            {
                NextUrl = $"{req.GetDisplayUrl()}{(req.QueryString.Value.StartsWith("?") ? "&" : "?")}{tokenQueryParamName}={ocrResult.ContinuationToken}"
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