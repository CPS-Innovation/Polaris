using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Search.Documents;
using Common.Domain.SearchIndex;
using Common.Factories.Contracts;
using Common.Logging;
using Microsoft.Extensions.Logging;

namespace Common.Clients
{
	public class SearchIndexClient : ISearchIndexClient
	{
		private readonly SearchClient _searchClient;
		private readonly IStreamlinedSearchResultFactory _streamlinedSearchResultFactory;
		private readonly ILogger<SearchIndexClient> _logger;

        public SearchIndexClient(ISearchClientFactory searchClientFactory, IStreamlinedSearchResultFactory streamlinedSearchResultFactory, ILogger<SearchIndexClient> logger)
		{
			_searchClient = searchClientFactory.Create();
			_streamlinedSearchResultFactory = streamlinedSearchResultFactory;
			_logger = logger;
		}

		public async Task<IList<StreamlinedSearchLine>> Query(int caseId, string searchTerm, Guid correlationId)
		{
			_logger.LogMethodEntry(correlationId, nameof(Query), $"CaseId '{caseId}', searchTerm '{searchTerm}'");
			
			var searchOptions = new SearchOptions
			{
				Filter = $"caseId eq {caseId}"
			};
			
			var searchResults = await _searchClient.SearchAsync<SearchLine>(searchTerm, searchOptions);

			var searchLines = new List<SearchLine>();
			await foreach (var searchResult in searchResults.Value.GetResultsAsync())
			{
				searchLines.Add(searchResult.Document);
			}

			_logger.LogMethodFlow(correlationId, nameof(Query), $"Found {searchLines.Count} results, building streamlined search results");
            var results = BuildStreamlinedResults(searchLines, searchTerm, correlationId);
            _logger.LogMethodExit(correlationId, nameof(Query), string.Empty);
            return results;
		}

        public IList<StreamlinedSearchLine> BuildStreamlinedResults(IList<SearchLine> searchResults, string searchTerm, Guid correlationId)
        {
	        _logger.LogMethodEntry(correlationId, nameof(BuildStreamlinedResults), string.Empty);
	        
            var streamlinedResults = new List<StreamlinedSearchLine>();
            if (searchResults.Count == 0)
                return streamlinedResults;

            streamlinedResults.AddRange(searchResults
	            .Select(searchResult => _streamlinedSearchResultFactory.Create(searchResult, searchTerm, correlationId)));

            _logger.LogMethodExit(correlationId, nameof(BuildStreamlinedResults), string.Empty);
            return streamlinedResults;
        }
    }
}
