using Common.Configuration;
using Common.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PolarisGateway.Services.Artefact;
using PolarisGateway.Services.Artefact.Domain;
using System.IO;
using System.Net;
using System.Threading.Tasks;


namespace PolarisGateway.Functions;

public class GetPdf : BaseFunction
{
    private const string PdfContentType = "application/pdf";
    private const string isOcrProcessedParamName = "isOcrProcessed";
    private const string ForceRefreshParamName = "ForceRefresh";
    private readonly ILogger<GetPdf> _logger;
    private readonly IPdfArtefactService _pdfArtefactService;

    public GetPdf(
        ILogger<GetPdf> logger,
        IPdfArtefactService pdfArtefactService)
        : base()
    {
        _logger = logger.ExceptionIfNull();
        _pdfArtefactService = pdfArtefactService.ExceptionIfNull();
    }

    [Function(nameof(GetPdf))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(GetPdf), tags: ["Documents"], Summary = "Get pdf", Description = "Gives the pdf")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter(name: "caseUrn", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Case URN", Description = "The URN identifier of the case")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("documentId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the document", Required = true)]
    [OpenApiParameter("versionId", In = ParameterLocation.Path, Type = typeof(long), Description = "The version Id of the document", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK,contentType: "application/pdf",bodyType: typeof(byte[]),Description = "Returns the generated PDF file")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnsupportedMediaType,contentType: "application/json",bodyType: typeof(ArtefactResult<Stream>),Description = "Returned when the PDF artefact is not available")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]

    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Pdf)] HttpRequest req, string caseUrn, int caseId, string documentId, long versionId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var isOcrProcessed = req.Query.ContainsKey(isOcrProcessedParamName) && bool.Parse(req.Query[isOcrProcessedParamName]);
        var forceRefresh = req.Query.ContainsKey(ForceRefreshParamName) && bool.Parse(req.Query[ForceRefreshParamName]);
        var getPdfResult = await _pdfArtefactService.GetPdfAsync(cmsAuthValues, correlationId, caseUrn, caseId, documentId, versionId, isOcrProcessed, forceRefresh);
        return getPdfResult.Status == ResultStatus.ArtefactAvailable ?
         new FileStreamResult(getPdfResult.Artefact, PdfContentType) :
         new JsonResult(getPdfResult)
         {
             StatusCode = getPdfResult.FailedHttpStatusCode,
         };
    }
}