using System.Linq;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Common.Telemetry;
using coordinator.TelemetryEvents;
using coordinator.Helpers;
using coordinator.Durable.Entity;
using coordinator.Mappers;
using coordinator.Durable.Payloads.Domain;
using Microsoft.AspNetCore.Http;
using coordinator.Clients.TextExtractor;
using Microsoft.DurableTask.Client;
using Microsoft.Azure.Functions.Worker;

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

        [Function(nameof(SearchCase))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseSearch)] HttpRequest req,
            string caseUrn,
            int caseId,
            [DurableClient] DurableTaskClient client)
        {
            var currentCorrelationId = req.Headers.GetCorrelationId();
            var searchTerm = req.Query[QueryStringSearchParam];

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new BadRequestObjectResult("Search term not supplied.");
            }

            var searchResults = await _textExtractorClient.SearchTextAsync(caseUrn, caseId, searchTerm, currentCorrelationId);

            var entityId = CaseDurableEntity.GetEntityId(caseId);
            var stateResponse = await client.Entities.GetEntityAsync<CaseDurableEntity>(entityId);

            if (stateResponse is not null && stateResponse?.IncludesState == true)
            {
                var entityState = stateResponse.State;
                // todo: temporary code, need an AllDocuments method as per first refactor
                var documents =
                    entityState.CmsDocuments.OfType<BaseDocumentEntity>()
                        .Concat(entityState.PcdRequests)
                        .Append(entityState.DefendantsAndCharges)
                        .Select(_searchFilterDocumentMapper.MapToSearchFilterDocument)
                        .ToList();

                var filteredSearchResults = searchResults
                    .Where(result => documents.Any(doc => doc.DocumentId == result.DocumentId && doc.VersionId == result.VersionId))
                    .ToList();

                var documentIds = filteredSearchResults
                    .Select(result => result.DocumentId)
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

                return new OkObjectResult(filteredSearchResults);
            }

            return new BadRequestObjectResult("Could not get entity state.");
        }
    }
}
