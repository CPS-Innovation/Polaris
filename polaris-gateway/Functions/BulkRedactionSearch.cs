using Common.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PolarisGateway.Services.Artefact;
using System.Threading;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class BulkRedactionSearch : BaseFunction
{
    private readonly ILogger<BulkRedactionSearch> _logger;
    private readonly IOcrArtefactService _ocrArtefactService;
    private const string SearchTextHeader = "SearchText";

    public BulkRedactionSearch(ILogger<BulkRedactionSearch> logger, IOcrArtefactService ocrArtefactService)
    {
        _logger = logger;
        _ocrArtefactService = ocrArtefactService;
    }

    [Function(nameof(BulkRedactionSearch))]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = RestApi.OcrSearch)] HttpRequest req, string caseUrn, int caseId, string documentId, long versionId, CancellationToken cancellationToken)
    {
        var searchText = req.Query[SearchTextHeader];
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var redactionDefinitionDtos = await _ocrArtefactService.GetOcrSearchRedactionsAsync(cmsAuthValues,
            correlationId, caseUrn, caseId, documentId, versionId, searchText, cancellationToken);

        return new OkObjectResult(redactionDefinitionDtos);
    }
}