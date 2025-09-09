using Common.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Common.Extensions;
using coordinator.Durable.Payloads;
using coordinator.Durable.Providers;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;

namespace coordinator.Functions;

public class BulkRedactionSearch
{
    private readonly ILogger<BulkRedactionSearch> _logger;
    private readonly IOrchestrationProvider _orchestrationProvider;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly IDdeiAuthClient _ddeiAuthClient;
    private const string SearchTextHeader = "SearchText";

    public BulkRedactionSearch(ILogger<BulkRedactionSearch> logger, IOrchestrationProvider orchestrationProvider, IDdeiArgFactory ddeiArgFactory, IDdeiAuthClient ddeiAuthClient)
    {
        _logger = logger;
        _orchestrationProvider = orchestrationProvider;
        _ddeiArgFactory = ddeiArgFactory;
        _ddeiAuthClient = ddeiAuthClient;
    }

    [Function("BulkRedactionSearch")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.OcrSearch)] HttpRequest req, string caseUrn, int caseId, string documentId, long versionId, CancellationToken cancellationToken, [DurableClient] DurableTaskClient orchestrationClient)
    {
        var currentCorrelationId = req.Headers.GetCorrelationId();
        var cmsAuthValues = req.Headers.GetCmsAuthValues();
        await _ddeiAuthClient.VerifyCmsAuthAsync(_ddeiArgFactory.CreateCmsCaseDataArgDto(cmsAuthValues, currentCorrelationId));
        var bulkRedactionPayload = new BulkRedactionPayload
        {
            CaseUrn = caseUrn,
            CaseId = caseId,
            DocumentId = documentId,
            VersionId = versionId,
            SearchText = req.Query[SearchTextHeader]
        };

        var isAccepted = await _orchestrationProvider.BulkSearchDocumentAsync(orchestrationClient, currentCorrelationId, bulkRedactionPayload, cancellationToken);
        return new OkObjectResult("Welcome to Azure Functions!");
    }
}