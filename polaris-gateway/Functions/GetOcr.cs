using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using Microsoft.AspNetCore.Http.Extensions;
using PolarisGateway.Services.Artefact;
using PolarisGateway.Services.Artefact.Domain;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using System;
using Common.Telemetry;


namespace PolarisGateway.Functions;

public class GetOcr : BaseFunction
{
    private const string JsonContentType = "application/json";
    private const string tokenQueryParamName = "token";
    private const string isOcrProcessedParamName = "isOcrProcessed";
    private readonly ILogger<GetOcr> _logger;
    private readonly IOcrArtefactService _ocrArtefactService;
    private readonly ITelemetryClient _telemetryClient;

    public GetOcr(
        ILogger<GetOcr> logger,
        IOcrArtefactService ocrArtefactService,
        ITelemetryClient telemetryClient)
        : base()
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ocrArtefactService = ocrArtefactService ?? throw new ArgumentNullException(nameof(ocrArtefactService));
        _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
    }

    [Function(nameof(GetOcr))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Ocr)] HttpRequest req, string caseUrn, int caseId, string documentId, long versionId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var isOcrProcessed = req.Query.ContainsKey(isOcrProcessedParamName) && bool.Parse(req.Query[isOcrProcessedParamName]);
        var token = req.Query.ContainsKey(tokenQueryParamName) ?
            Guid.Parse(req.Query[tokenQueryParamName]) :
            (Guid?)null;

        var ocrResult = await _ocrArtefactService.GetOcrAsync(cmsAuthValues, correlationId, caseUrn, caseId, documentId, versionId, isOcrProcessed, token);
        return ocrResult.Status switch
        {
            ResultStatus.ArtefactAvailable => new JsonResult(
                ocrResult.Artefact
            )
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