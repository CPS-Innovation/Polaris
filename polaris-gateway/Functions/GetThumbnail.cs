using Common.Configuration;
using Common.Domain.Pii;
using Common.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PolarisGateway.Clients.PdfThumbnailGenerator;
using PolarisGateway.Extensions;
using PolarisGateway.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class GetThumbnail : BaseFunction
{
    private readonly ILogger<GetThumbnail> _logger;
    private readonly IPdfThumbnailGeneratorClient _pdfThumbnailGeneratorClient;
    private readonly ITelemetryClient _telemetryClient;

    public GetThumbnail(
        ILogger<GetThumbnail> logger,
        IPdfThumbnailGeneratorClient pdfThumbnailGeneratorClient,
        ITelemetryClient telemetryClient)
        : base()
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _pdfThumbnailGeneratorClient = pdfThumbnailGeneratorClient ?? throw new ArgumentNullException(nameof(pdfThumbnailGeneratorClient));
        _telemetryClient = telemetryClient;
    }

    [Function(nameof(GetThumbnail))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(GetThumbnail), tags: ["Documents"], Summary = "Get Thumbnail", Description = "Gives the thumbnail")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter(name: "caseUrn", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Case URN", Description = "The URN identifier of the case")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("documentId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the document", Required = true)]
    [OpenApiParameter("versionId", In = ParameterLocation.Path, Type = typeof(long), Description = "The version Id of the document", Required = true)]
    [OpenApiParameter("maxDimensionPixel", In = ParameterLocation.Path, Type = typeof(int), Description = "The max Dimension Pixel of the document", Required = true)]
    [OpenApiParameter("pageIndex", In = ParameterLocation.Path, Type = typeof(int), Description = "The page index of the document", Required = true)]

    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(object), Description = "OCR processing completed successfully")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]


    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Thumbnail)] HttpRequest req,
        string caseUrn, int caseId, string documentId, int versionId, int maxDimensionPixel, int pageIndex)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);
        return await (await _pdfThumbnailGeneratorClient.GetThumbnailAsync(caseUrn, caseId, documentId, versionId, maxDimensionPixel, pageIndex, cmsAuthValues, correlationId)).ToActionResult();
    }
}