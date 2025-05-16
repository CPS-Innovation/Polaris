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


namespace PolarisGateway.Functions;

public class GetPii : BaseFunction
{
    private const string tokenQueryParamName = "token";
    private const string isOcrProcessedParamName = "isOcrProcessed";
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
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Pii)] HttpRequest req, string caseUrn, int caseId, string documentId, long versionId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var isOcrProcessed = req.Query.ContainsKey(isOcrProcessedParamName) && bool.Parse(req.Query[isOcrProcessedParamName]);
        var token = req.Query.ContainsKey(tokenQueryParamName) ?
            Guid.Parse(req.Query[tokenQueryParamName]) :
            (Guid?)null;

        var ocrResult = await _piiArtefactService.GetPiiAsync(cmsAuthValues, correlationId, caseUrn, caseId, documentId, versionId, isOcrProcessed, token);
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