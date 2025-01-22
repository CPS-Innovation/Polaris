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
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Thumbnail)] HttpRequest req,
        string caseUrn, int caseId, string documentId, int versionId, int maxDimensionPixel, int pageIndex)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);
        return await (await _pdfThumbnailGeneratorClient.GetThumbnailAsync(caseUrn, caseId, documentId, versionId, maxDimensionPixel, pageIndex, cmsAuthValues, correlationId)).ToActionResult();
    }
}