using Common.Configuration;
using Common.Dto.Request;
using Common.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PolarisGateway.Clients.PdfThumbnailGenerator;
using PolarisGateway.Extensions;
using System;
using System.Net;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class GenerateThumbnail : BaseFunction
{
    private readonly ILogger<GenerateThumbnail> _logger;
    private readonly IPdfThumbnailGeneratorClient _pdfThumbnailGeneratorClient;
    private readonly ITelemetryClient _telemetryClient;

    public GenerateThumbnail(
        ILogger<GenerateThumbnail> logger,
        IPdfThumbnailGeneratorClient pdfThumbnailGeneratorClient,
        ITelemetryClient telemetryClient)
        : base()
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _pdfThumbnailGeneratorClient = pdfThumbnailGeneratorClient ?? throw new ArgumentNullException(nameof(pdfThumbnailGeneratorClient));
        _telemetryClient = telemetryClient;
    }

    [Function(nameof(GenerateThumbnail))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    [OpenApiOperation(operationId: nameof(GenerateThumbnail), tags: ["Documents"], Summary = "Generate Thumbnail", Description = "Generate Thumbnail")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter(name: "caseUrn", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Case URN", Description = "The URN identifier of the case")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("documentId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the document", Required = true)]
    [OpenApiParameter("versionId", In = ParameterLocation.Path, Type = typeof(long), Description = "The version Id of the document", Required = true)]
    [OpenApiParameter("maxDimensionPixel", In = ParameterLocation.Path, Type = typeof(int), Description = "The max Dimension Pixel in the document to generate thumbnail", Required = true)]
    [OpenApiParameter("pageIndex", In = ParameterLocation.Path, Type = typeof(int), Description = "The page Index of the document to generate thumbnail", Required = false)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AddDocumentNoteRequestDto), Summary = "Case found", Description = "Returns case details")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.GenerateThumbnail)] HttpRequest req,
        string caseUrn, int caseId, string documentId, int versionId, int maxDimensionPixel, int? pageIndex)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        return await (await _pdfThumbnailGeneratorClient.GenerateThumbnailAsync(caseUrn, caseId, documentId, versionId, maxDimensionPixel, pageIndex, cmsAuthValues, correlationId)).ToActionResult();
    }
}