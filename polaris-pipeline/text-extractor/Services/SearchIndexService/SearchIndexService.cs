using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Common.Domain.SearchIndex;
using Common.Dto.Response;
using Common.Logging;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;
using text_extractor.Factories.Contracts;
using text_extractor.Mappers.Contracts;
using text_extractor.Services.CaseSearchService;

namespace text_extractor.Services.SearchIndexService
{
    public class SearchIndexService : ISearchIndexService
    {
        private const long MaximumIndexRetrievalSize = 20000;
        private readonly SearchClient _azureSearchClient;
        private readonly ISearchLineFactory _searchLineFactory;
        private readonly ISearchIndexingBufferedSenderFactory _searchIndexingBufferedSenderFactory;
        private readonly IStreamlinedSearchResultFactory _streamlinedSearchResultFactory;
        private readonly ILineMapper _lineMapper;
        private readonly ILogger<SearchIndexService> _logger;

        public SearchIndexService(
            IAzureSearchClientFactory searchClientFactory,
            ISearchLineFactory searchLineFactory,
            ISearchIndexingBufferedSenderFactory searchIndexingBufferedSenderFactory,
            IStreamlinedSearchResultFactory streamlinedSearchResultFactory,
            ILineMapper lineMapper,
            ILogger<SearchIndexService> logger)
        {
            _azureSearchClient = searchClientFactory.Create();
            _searchLineFactory = searchLineFactory;
            _searchIndexingBufferedSenderFactory = searchIndexingBufferedSenderFactory;
            _streamlinedSearchResultFactory = streamlinedSearchResultFactory;
            _lineMapper = lineMapper;
            _logger = logger;
        }

        public async Task<int> SendStoreResultsAsync(AnalyzeResults analyzeResults, int caseId, string documentId, long versionId, Guid correlationId)
        {
            var lines = new List<SearchLine>();

            foreach (var readResult in analyzeResults.ReadResults)
            {
                var searchLines = readResult.Lines.Select((line, index) => _searchLineFactory.Create(
                    caseId,
                    documentId,
                    versionId,
                    "deprecated",
                    readResult,
                    _lineMapper.Map(line),
                    index));

                lines.AddRange(searchLines);
            }

            if (lines.Count == 0)
            {
                return 0;
            }

            await using var indexer = _searchIndexingBufferedSenderFactory.Create(_azureSearchClient);

            var indexTaskCompletionSource = new TaskCompletionSource<bool>();
            var statuses = new HashSet<int?>();

            indexer.ActionFailed += error =>
            {
                if (error.Exception is RequestFailedException exception)
                {
                    var status = exception.Status; 
                    statuses.Add(status);
                }

                if (!indexTaskCompletionSource.Task.IsCompleted)
                {
                    indexTaskCompletionSource.SetResult(false);
                }

                return Task.CompletedTask;
            };

            var successCount = 0;
            indexer.ActionCompleted += _ =>
            {
                successCount++;
                if (successCount == lines.Count)
                {
                    indexTaskCompletionSource.SetResult(true);
                }

                return Task.CompletedTask;
            };

            await indexer.UploadDocumentsAsync(lines);
            await indexer.FlushAsync();

            if (!await indexTaskCompletionSource.Task)
            {
                throw new RequestFailedException($"At least one indexing action failed. Status(es) = {string.Join(", ", statuses)}");
            }

            _logger.LogMethodFlow(correlationId, nameof(SendStoreResultsAsync), $"Case: {caseId}, Document: {documentId}, Version: {versionId}, indexed {lines.Count} lines");
            return lines.Count;
        }

        public async Task<IList<StreamlinedSearchLine>> QueryAsync(int caseId, string searchTerm)
        {
            var filter = $"caseId eq {caseId}";
            var searchOptions = new SearchOptions
            {
                Filter = filter,
                SessionId = caseId.ToString()
            };

            // => e.g. search=caseId eq 2146928
            var searchResults = await GetSearchResults<SearchLine>(searchOptions, searchTerm);
            var searchLines = new List<SearchLine>();

            await foreach (var searchResult in searchResults.Value.GetResultsAsync())
            {
                searchLines.Add(searchResult.Document);
            }

            var streamlinedResults = new List<StreamlinedSearchLine>();
            if (searchLines.Count == 0)
            {
                return streamlinedResults;
            }

            var searchResultsValues = searchLines.Select(searchResult => _streamlinedSearchResultFactory.Create(searchResult, searchTerm));

            streamlinedResults.AddRange(searchResultsValues);

            return streamlinedResults;
        }

        public async Task<IndexDocumentsDeletedResult> RemoveCaseIndexEntriesAsync(int caseId, Guid correlationId)
        {
            if (caseId == 0)
            {
                throw new ArgumentException("Invalid caseId", nameof(caseId));
            }

            var indexCountSearchOptions = new SearchOptions
            {
                Filter = $"caseId eq {caseId}",
                IncludeTotalCount = true,
                Size = 0,
                Select = { "id" },
                SessionId = caseId.ToString()
            };

            var countResult = await GetSearchResults<SearchLineId>(indexCountSearchOptions);
            if (countResult.Value.TotalCount == null) return IndexDocumentsDeletedResult.Empty();

            var indexTotal = countResult.Value.TotalCount.Value;
            if (indexTotal == 0)
                return IndexDocumentsDeletedResult.Empty();
                
            var result = new IndexDocumentsDeletedResult();
            var indexesToProcess = indexTotal;

            while (indexesToProcess > 0)
            {
                var indexSize = indexesToProcess;

                if (indexSize > MaximumIndexRetrievalSize) indexSize = MaximumIndexRetrievalSize;

                var deletionResult = await DeleteDocumentIndexes(caseId, indexSize);

                result.DocumentCount = indexTotal;
                result.SuccessCount += deletionResult.SuccessCount;
                result.FailureCount += deletionResult.FailureCount;

                indexesToProcess -= indexSize;
            }
            _logger.LogMethodFlow(correlationId, nameof(RemoveCaseIndexEntriesAsync), $"Case: {caseId}, removed {indexTotal} lines");
            return result;
        }

        public async Task<SearchIndexCountResult> GetCaseIndexCount(int caseId, Guid correlationId)
        {
            if (caseId == 0)
            {
                throw new ArgumentException("Invalid caseId", nameof(caseId));
            }

            var indexCountSearchOptions = new SearchOptions
            {
                Filter = $"caseId eq {caseId}",
                IncludeTotalCount = true,
                Size = 0,
                SessionId = caseId.ToString()
            };

            var countResult = await GetSearchResults<SearchLineId>(indexCountSearchOptions);
            if (countResult.Value.TotalCount == null) return new SearchIndexCountResult(0);

            var indexTotal = countResult.Value.TotalCount.Value;

            _logger.LogMethodFlow(correlationId, nameof(GetCaseIndexCount), $"Case: {caseId}, counted {indexTotal} lines");
            return new SearchIndexCountResult(indexTotal);
        }

        public async Task<SearchIndexCountResult> GetDocumentIndexCount(int caseId, string documentId, long versionId, Guid correlationId)
        {
            if (caseId == 0)
            {
                throw new ArgumentException("Invalid caseId", nameof(caseId));
            }

            var indexCountSearchOptions = new SearchOptions
            {
                Filter = $"caseId eq {caseId} and documentId eq '{documentId}' and versionId eq {versionId}",
                IncludeTotalCount = true,
                Size = 0,
                SessionId = caseId.ToString()
            };

            var countResult = await GetSearchResults<SearchLineId>(indexCountSearchOptions);
            if (countResult.Value.TotalCount == null) return new SearchIndexCountResult(0);

            var indexTotal = countResult.Value.TotalCount.Value;

            _logger.LogMethodFlow(correlationId, nameof(GetDocumentIndexCount), $"Case: {caseId}, Document: {documentId}, Version: {versionId},  counted {indexTotal} lines");
            return new SearchIndexCountResult(indexTotal);
        }

        private async Task<Response<SearchResults<TSearchable>>> GetSearchResults<TSearchable>(SearchOptions searchOptions, string searchTerm = "*")
        {
            return await _azureSearchClient.SearchAsync<TSearchable>(searchTerm, searchOptions);
        }

        private async Task<IndexDocumentsDeletedResult> DeleteDocumentIndexes(int caseId, long indexCount)
        {
            var searchOptions = new SearchOptions
            {
                Filter = $"caseId eq {caseId}",
                Size = (int)indexCount,
                Select = { "id" },
                SessionId = caseId.ToString()
            };

            var results = await GetSearchResults<SearchLineId>(searchOptions);
            var searchLines = new List<SearchLineId>();
            await foreach (var searchResult in results.Value.GetResultsAsync())
            {
                searchLines.Add(searchResult.Document);
            }

            if (searchLines.Count == 0)
            {
                return IndexDocumentsDeletedResult.Empty();
            }

            await using var indexer = _searchIndexingBufferedSenderFactory.Create(_azureSearchClient);
            var indexTaskCompletionSource = new TaskCompletionSource<bool>();

            var failureCount = 0;
            indexer.ActionFailed += _ =>
            {
                failureCount++;
                if (!indexTaskCompletionSource.Task.IsCompleted)
                {
                    indexTaskCompletionSource.SetResult(false);
                }

                return Task.CompletedTask;
            };

            var successCount = 0;
            indexer.ActionCompleted += _ =>
            {
                successCount++;
                if (successCount == searchLines.Count)
                {
                    indexTaskCompletionSource.SetResult(true);
                }

                return Task.CompletedTask;
            };

            await indexer.DeleteDocumentsAsync(searchLines);
            await indexer.FlushAsync();

            if (!await indexTaskCompletionSource.Task)
            {
                throw new RequestFailedException("At least one indexing action failed.");
            }

            return new IndexDocumentsDeletedResult
            {
                DocumentCount = searchLines.Count,
                SuccessCount = successCount,
                FailureCount = failureCount
            };
        }
    }
}
