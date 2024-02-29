using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using coordinator.Clients.Contracts;
using Common.Configuration;
using Common.Constants;
using Common.Logging;
using coordinator.Functions.Orchestration.Functions.Case;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Telemetry.Contracts;
using coordinator.TelemetryEvents;
using coordinator.Helpers.ChunkHelper;
using coordinator.Durable.Entity;
using coordinator.Domain.Entity;
using coordinator.Mappers;

namespace coordinator.Functions.DurableEntity.Client.Case
{
    public class SearchCaseClient
    {
        private readonly ITextExtractorClient _textExtractorClient;
        private readonly ISearchFilterDocumentMapper _searchFilterDocumentMapper;
        private readonly ITelemetryClient _telemetryClient;

        public SearchCaseClient(ITextExtractorClient textExtractorClient, ISearchFilterDocumentMapper searchFilterDocumentMapper, ITelemetryClient telemetryClient)
        {
            _textExtractorClient = textExtractorClient;
            _searchFilterDocumentMapper = searchFilterDocumentMapper;
            _telemetryClient = telemetryClient;
        }

        const string loggingName = $"{nameof(SearchCaseClient)} - {nameof(HttpStart)}";
        const string correlationErrorMessage = "Invalid correlationId. A valid GUID is required.";

        [FunctionName(nameof(SearchCaseClient))]
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

                var searchTerm = req.RequestUri.ParseQueryString()["query"];
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return new BadRequestObjectResult("Search term not supplied.");

                req.Headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIdValues);
                if (correlationIdValues == null)
                {
                    return new BadRequestObjectResult(correlationErrorMessage);
                }

                var correlationId = correlationIdValues.FirstOrDefault();
                if (!Guid.TryParse(correlationId, out currentCorrelationId))
                    if (currentCorrelationId == Guid.Empty)
                    {
                        return new BadRequestObjectResult(correlationErrorMessage);
                    }


                var entityId = new EntityId(nameof(CaseDurableEntity), RefreshCaseOrchestrator.GetKey(caseId.ToString()));
                var trackerState = await client.ReadEntityStateAsync<CaseDurableEntity>(entityId);

                if (!trackerState.EntityExists)
                {
                    var baseMessage = $"No pipeline tracker found with id '{caseId}'";
                    return new NotFoundObjectResult(baseMessage);
                }

                CaseDurableEntity entityState = trackerState.EntityState;
                var documents =
                    entityState.CmsDocuments.OfType<BaseDocumentEntity>()
                        .Concat(entityState.PcdRequests)
                        .Append(entityState.DefendantsAndCharges)
                        .Select(_searchFilterDocumentMapper.MapToSearchFilterDocument)
                        .ToList();

                var searchResults = await _textExtractorClient.SearchTextAsync(caseUrn, caseId, searchTerm, currentCorrelationId, documents);

                var documentIds = searchResults.Select(result => result.PolarisDocumentId).Distinct().ToList();

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
                log.LogMethodError(currentCorrelationId, loggingName, ex.Message, ex);
                return new StatusCodeResult(500);
            }

        }
    }
}
