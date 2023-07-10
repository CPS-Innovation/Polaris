using System.Net.Http;
using System.Threading.Tasks;
using Common.Services.CaseSearchService.Contracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace text_extractor.Functions
{
    public class SearchText
    {
        private readonly ISearchIndexService _searchIndexService;

        public SearchText(ISearchIndexService searchIndexService)
        {
            _searchIndexService = searchIndexService;
        }

        [FunctionName(nameof(SearchText))]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "search")] HttpRequestMessage request)
        {
            var searchResults = await _searchIndexService.QueryAsync(caseId, documents, searchTerm, currentCorrelationId);
            return new OkObjectResult(searchResults);
        }
    }
}