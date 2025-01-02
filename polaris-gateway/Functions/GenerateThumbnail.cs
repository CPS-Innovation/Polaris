using Common.Configuration;
using Common.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PolarisGateway.Clients.PdfThumbnailGenerator;
using PolarisGateway.Extensions;
using System;
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
        : base(telemetryClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _pdfThumbnailGeneratorClient = pdfThumbnailGeneratorClient ?? throw new ArgumentNullException(nameof(pdfThumbnailGeneratorClient));
        _telemetryClient = telemetryClient;
    }

    [Function(nameof(GenerateThumbnail))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status423Locked)]

    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.GenerateThumbnail)] HttpRequest req,
        string caseUrn, int caseId, string documentId, int versionId, int maxDimensionPixel, int? pageIndex)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        return await (await _pdfThumbnailGeneratorClient.GenerateThumbnailAsync(caseUrn, caseId, documentId, versionId, maxDimensionPixel, pageIndex, cmsAuthValues, correlationId)).ToActionResult();
    }
}