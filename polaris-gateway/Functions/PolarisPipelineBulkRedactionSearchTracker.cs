using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using PolarisGateway.Clients.Coordinator;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using PolarisGateway.Extensions;
using Common.Telemetry;

namespace PolarisGateway.Functions;

public class PolarisPipelineBulkRedactionSearchTracker : BaseFunction
{
    private readonly ILogger<PolarisPipelineBulkRedactionSearchTracker> _logger;
    private readonly ICoordinatorClient _coordinatorClient;
    private const string SearchTextHeader = "SearchText";

    public PolarisPipelineBulkRedactionSearchTracker(
        ILogger<PolarisPipelineBulkRedactionSearchTracker> logger,
        ICoordinatorClient coordinatorClient)
    {
        _logger = logger;
        _coordinatorClient = coordinatorClient;
    }

    [Function(nameof(PolarisPipelineBulkRedactionSearchTracker))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.OcrSearchTracker)] HttpRequest req, string caseUrn, int caseId, string documentId, long versionId, CancellationToken cancellationToken)
    {
        var searchText = req.Query[SearchTextHeader];
        var correlationId = EstablishCorrelation(req);

        return await (await _coordinatorClient.GetTrackerBulkRedactionSearchAsync(
                caseUrn,
                caseId,
                documentId,
                versionId,
                searchText,
                correlationId, 
                cancellationToken))
            .ToActionResult();
    }
}