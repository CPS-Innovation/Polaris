using Common.Configuration;
using Common.Extensions;
using coordinator.Domain;
using coordinator.Durable.Payloads;
using coordinator.Durable.Providers;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

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

    [Function(nameof(BulkRedactionSearch))]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.OcrSearch)] HttpRequest req, string caseUrn, int caseId, string documentId, long versionId, CancellationToken cancellationToken, [DurableClient] DurableTaskClient orchestrationClient)
    {
        var currentCorrelationId = req.Headers.GetCorrelationId();
        var cmsAuthValues = req.Headers.GetCmsAuthValues();
        await _ddeiAuthClient.VerifyCmsAuthAsync(_ddeiArgFactory.CreateCmsCaseDataArgDto(cmsAuthValues, currentCorrelationId));
        var searchText = req.Query[SearchTextHeader];
        var bulkRedactionPayload = new BulkRedactionSearchPayload
        {
            CaseUrn = caseUrn,
            CaseId = caseId,
            DocumentId = documentId,
            VersionId = versionId,
            SearchText = searchText,
            CmsAuthDetails = cmsAuthValues,
            CorrelationId = currentCorrelationId
        };

        var isAccepted = await _orchestrationProvider.BulkSearchDocumentAsync(orchestrationClient, bulkRedactionPayload, cancellationToken);
        return new ObjectResult(new BulkRedactionSearchResponse(caseUrn, caseId, documentId, versionId, searchText))
        {
            StatusCode = isAccepted ? StatusCodes.Status200OK : StatusCodes.Status423Locked
        };
    }
}