using Common.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Extensions;
using System.Threading;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class PolarisPipelineBulkRedactionSearch : BaseFunction
{
    private readonly ILogger<PolarisPipelineBulkRedactionSearch> _logger;
    private readonly ICoordinatorClient _coordinatorClient;
    private const string SearchTextHeader = "SearchText";

    public PolarisPipelineBulkRedactionSearch(ILogger<PolarisPipelineBulkRedactionSearch> logger, ICoordinatorClient coordinatorClient)
    {
        _logger = logger;
        _coordinatorClient = coordinatorClient;
    }

    [Function(nameof(PolarisPipelineBulkRedactionSearch))]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.OcrSearch)] HttpRequest req, string caseUrn, int caseId, string documentId, long versionId, CancellationToken cancellationToken)
    {
        var searchText = req.Query[SearchTextHeader];
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        return await (await _coordinatorClient.BulkRedactionSearchAsync(caseUrn, caseId, documentId, versionId, searchText, correlationId, cmsAuthValues, cancellationToken)).ToActionResult();
    }
}