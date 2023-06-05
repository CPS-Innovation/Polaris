using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Search.Documents;
using Common.Clients.Contracts;
using Common.Domain.Entity;
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

		public async Task<IList<StreamlinedSearchLine>> Query(int caseId, List<BaseDocumentEntity> documents, string searchTerm, Guid correlationId)
		{
			_logger.LogMethodEntry(correlationId, nameof(Query), $"CaseId '{caseId}', searchTerm '{searchTerm}'");

			var filter = GetSearchQuery(caseId, documents);
			var searchOptions = new SearchOptions
			{
				Filter = filter
            };
			
			var searchResults = await _searchClient.SearchAsync<SearchLine>(searchTerm, searchOptions);
			var searchLines = new List<SearchLine>();
			await foreach (var searchResult in searchResults.Value.GetResultsAsync())
			{
                if (IsLiveDocumentResult(documents, searchResult.Document))
                {
                    searchLines.Add(searchResult.Document);
                }
			}

			_logger.LogMethodFlow(correlationId, nameof(Query), $"Found {searchLines.Count} results, building streamlined search results");
            var results = BuildStreamlinedResults(searchLines, searchTerm, correlationId);
            _logger.LogMethodExit(correlationId, nameof(Query), string.Empty);
            return results;
		}

        private string GetSearchQuery(int caseId, List<BaseDocumentEntity> documents)
        {
            var stringBuilder = new StringBuilder($"caseId eq {caseId}");

			if(documents.Any())
			{
				stringBuilder.Append(" and (");
				stringBuilder.Append($"versionId eq {documents[0].CmsVersionId}");
				for( var i = 1;  i < documents.Count; i++ )
				{
                    stringBuilder.Append($" or versionId eq {documents[i].CmsVersionId}");
                }
                stringBuilder.Append(")");
            }
			return stringBuilder.ToString();
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

        private bool IsLiveDocumentResult(List<BaseDocumentEntity> documents, SearchLine searchLine)
        {
            // SearchLineFactory => {cmsCaseId}:{polarisDocumentId}:{readResult.Page}:{index}

            var decodedSearchLineId = Encoding.UTF8.GetString(Convert.FromBase64String(searchLine.Id));
            var elements = decodedSearchLineId.Split(":");
            if (elements.Length < 2)
                return false;
            var resultPolarisDocumentIdValue = elements[1];

            return documents.Any(document => document.PolarisDocumentId.Value == resultPolarisDocumentIdValue);
        }
    }
}
