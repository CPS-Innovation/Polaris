using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using coordinator.Clients;
using Common.Configuration;
using Common.Constants;
using Common.Logging;
using Common.Extensions;
using coordinator.Durable.Orchestration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Telemetry.Contracts;
using coordinator.TelemetryEvents;
using coordinator.Helpers.ChunkHelper;
using coordinator.Durable.Entity;
using coordinator.Mappers;
using coordinator.Durable.Payloads.Domain;

namespace coordinator.Functions
{
    public class SearchCase
    {
        private readonly ITextExtractorClient _textExtractorClient;
        private readonly ISearchFilterDocumentMapper _searchFilterDocumentMapper;
        private readonly ITelemetryClient _telemetryClient;

        public SearchCase(ITextExtractorClient textExtractorClient, ISearchFilterDocumentMapper searchFilterDocumentMapper, ITelemetryClient telemetryClient)
        {
            _textExtractorClient = textExtractorClient;
            _searchFilterDocumentMapper = searchFilterDocumentMapper;
            _telemetryClient = telemetryClient;
        }

        [FunctionName(nameof(SearchCase))]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = RestApi.CaseSearch)] HttpRequestMessage req,
            string caseUrn,
            int caseId,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();
                var searchTerm = req.RequestUri.ParseQueryString()["query"];
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
                log.LogMethodError(currentCorrelationId, nameof(SearchCase), ex.Message, ex);
                return new StatusCodeResult(500);
            }

        }
    }
}
