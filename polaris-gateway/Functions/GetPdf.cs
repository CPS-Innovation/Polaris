using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using PolarisGateway.Services.Artefact;
using PolarisGateway.Services.Artefact.Domain;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using System;
using Common.Telemetry;


namespace PolarisGateway.Functions;

public class GetPdf : BaseFunction
{
    private const string PdfContentType = "application/pdf";
    private const string isOcrProcessedParamName = "isOcrProcessed";
    private readonly ILogger<GetPdf> _logger;
    private readonly IPdfArtefactService _pdfArtefactService;

    public GetPdf(
        ILogger<GetPdf> logger,
        IPdfArtefactService pdfArtefactService,
        ITelemetryClient telemetryClient)
        : base()
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _pdfArtefactService = pdfArtefactService ?? throw new ArgumentNullException(nameof(pdfArtefactService));
    }

    [Function(nameof(GetPdf))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Pdf)] HttpRequest req, string caseUrn, int caseId, string documentId, long versionId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var isOcrProcessed = req.Query.ContainsKey(isOcrProcessedParamName) && bool.Parse(req.Query[isOcrProcessedParamName]);
        var getPdfResult = await _pdfArtefactService.GetPdfAsync(cmsAuthValues, correlationId, caseUrn, caseId, documentId, versionId, isOcrProcessed);
        return getPdfResult.Status == ResultStatus.ArtefactAvailable ?
            new FileStreamResult(getPdfResult.Artefact, PdfContentType) :
            new JsonResult(getPdfResult)
            {
                StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
            };
    }
}