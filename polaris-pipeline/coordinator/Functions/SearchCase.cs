using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using coordinator.Clients;
using Common.Configuration;
using Common.Logging;
using coordinator.Durable.Entity;
using coordinator.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Extensions;

namespace coordinator.Functions
{
    public class SearchCase
    {
        private readonly ITextExtractorClient _textExtractorClient;

        private readonly ISearchFilterDocumentMapper _searchFilterDocumentMapper;

        public SearchCase(ITextExtractorClient textExtractorClient, ISearchFilterDocumentMapper searchFilterDocumentMapper)
        {
            _textExtractorClient = textExtractorClient;
            _searchFilterDocumentMapper = searchFilterDocumentMapper;
        }

        [FunctionName(nameof(SearchCase))]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = RestApi.CaseSearch)] HttpRequestMessage req,
            string caseUrn,
            long caseId,
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

                var entityState = await CaseDurableEntity.GetReadOnlyEntityState(client, caseId.ToString());
                var documents = entityState.AllDocuments()
                        .Select(_searchFilterDocumentMapper.MapToSearchFilterDocument)
                        .ToList();

                var searchResults = await _textExtractorClient.SearchTextAsync(caseUrn, caseId, searchTerm, currentCorrelationId, documents);

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
