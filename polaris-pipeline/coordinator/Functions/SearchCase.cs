using System;
using System.Linq;
using System.Threading.Tasks;
using coordinator.Clients;
using Common.Configuration;
using Common.Extensions;
using coordinator.Durable.Orchestration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Telemetry.Contracts;
using coordinator.TelemetryEvents;
using coordinator.Helpers;
using coordinator.Durable.Entity;
using coordinator.Mappers;
using coordinator.Durable.Payloads.Domain;
using Microsoft.AspNetCore.Http;

namespace coordinator.Functions
{
    public class SearchCase
    {
        private const string QueryStringSearchParam = "query";
        private readonly ITextExtractorClient _textExtractorClient;
        private readonly ISearchFilterDocumentMapper _searchFilterDocumentMapper;
        private readonly ITelemetryClient _telemetryClient;
        private readonly ILogger<SearchCase> _logger;

        public SearchCase(
            ITextExtractorClient textExtractorClient,
            ISearchFilterDocumentMapper searchFilterDocumentMapper,
            ITelemetryClient telemetryClient,
            ILogger<SearchCase> logger)
        {
            _textExtractorClient = textExtractorClient;
            _searchFilterDocumentMapper = searchFilterDocumentMapper;
            _telemetryClient = telemetryClient;
            _logger = logger;
        }

        [FunctionName(nameof(SearchCase))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = RestApi.CaseSearch)] HttpRequest req,
            string caseUrn,
            int caseId,
            [DurableClient] IDurableEntityClient client)
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();
                var searchTerm = req.Query[QueryStringSearchParam];
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new BadRequestObjectResult("Search term not supplied.");
                }

                var entityId = new EntityId(nameof(CaseDurableEntity), RefreshCaseOrchestrator.GetKey(caseId.ToString()));
                var trackerState = await client.ReadEntityStateAsync<CaseDurableEntity>(entityId);

                var entityState = trackerState.EntityState;
                // todo: temporary code, need an AllDocuments method as per first refactor
                var documents =
                    entityState.CmsDocuments.OfType<BaseDocumentEntity>()
                        .Concat(entityState.PcdRequests)
                        .Append(entityState.DefendantsAndCharges)
                        .Select(_searchFilterDocumentMapper.MapToSearchFilterDocument)
                        .ToList();

                var searchResults = await _textExtractorClient.SearchTextAsync(caseUrn, caseId, searchTerm, currentCorrelationId, documents);

                var documentIds = searchResults
                    .Select(result => result.PolarisDocumentId)
                    .Distinct()
                    .ToList();

                // the max string length of Application Insights custom properties is 8192
                // so we chunk the docIds and create multiple events as some cases could exceed this limit
                var chunkedDocumentIds = ChunkHelper.ChunkStringListByMaxCharacterCount(documentIds, 8192);

                foreach (var documentIdsChunk in chunkedDocumentIds)
                {
                    var telemetryEvent = new SearchCaseEvent(
                        correlationId: currentCorrelationId,
                        caseId,
                        documentIds: documentIdsChunk
                    );
                    _telemetryClient.TrackEvent(telemetryEvent);
                }

                return new OkObjectResult(searchResults);
            }
            catch (Exception ex)
            {
                return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(SearchCase), currentCorrelationId, ex);
            }

        }
    }
}
