using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents;
using Common.Domain.SearchIndex;
using Common.Factories.Contracts;
using Common.Services.CaseSearchService.Contracts;
using Common.ValueObjects;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;

namespace Common.Services.CaseSearchService
{
    public class SearchIndexService : ISearchIndexService
    {
        private const int IndexSettleUnitDelayMs = 250;
        private const int IndexSettleRetryAttemptCount = 11;
        private readonly SearchClient _azureSearchClient;
        private readonly ISearchLineFactory _searchLineFactory;
        private readonly ISearchIndexingBufferedSenderFactory _searchIndexingBufferedSenderFactory;
        private readonly IStreamlinedSearchResultFactory _streamlinedSearchResultFactory;
        private readonly ILogger<SearchIndexService> _logger;

        public SearchIndexService(
            IAzureSearchClientFactory searchClientFactory,
            ISearchLineFactory searchLineFactory,
            ISearchIndexingBufferedSenderFactory searchIndexingBufferedSenderFactory,
            IStreamlinedSearchResultFactory streamlinedSearchResultFactory,
            ILogger<SearchIndexService> logger)
        {
            _azureSearchClient = searchClientFactory.Create();
            _searchLineFactory = searchLineFactory;
            _searchIndexingBufferedSenderFactory = searchIndexingBufferedSenderFactory;
            _streamlinedSearchResultFactory = streamlinedSearchResultFactory;
            _logger = logger;
        }

        public async Task SendStoreResultsAsync(AnalyzeResults analyzeResults, PolarisDocumentId polarisDocumentId, long cmsCaseId, string cmsDocumentId, long versionId, string blobPath, Guid correlationId)
        {
            var blobName = Path.GetFileName(blobPath);
            var lines = new List<SearchLine>();

            foreach (var readResult in analyzeResults.ReadResults)
            {
                var searchLines = readResult.Lines.Select
                    (
                        (line, index) => _searchLineFactory.Create
                                            (
                                                cmsCaseId,
                                                cmsDocumentId,
                                                polarisDocumentId,
                                                versionId,
                                                blobName,
                                                readResult,
                                                line,
                                                index
                                             )
                    );
                lines.AddRange(searchLines);
            }

            if (lines.Count == 0)
            {
                return;
            }

            await using var indexer = _searchIndexingBufferedSenderFactory.Create(_azureSearchClient);

            var indexTaskCompletionSource = new TaskCompletionSource<bool>();
            var statuses = new HashSet<int?>();

            var failureCount = 0;
            indexer.ActionFailed += error =>
            {
                if (error.Exception is RequestFailedException)
                {
                    var status = ((RequestFailedException)error.Exception)?.Status;
                    if (status != null)
                        statuses.Add(status);
                }

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
        }

        public async Task<IndexSettledResult> WaitForStoreResultsAsync(long cmsCaseId, string cmsDocumentId, long versionId, long targetCount)
        {
            var filter = $"caseId eq {cmsCaseId} and documentId eq '{cmsDocumentId}' and versionId eq {versionId}";
            return await WaitForIndexCountResultsAsync(filter, targetCount);
        }

        public async Task<IndexSettledResult> WaitForCaseEmptyResultsAsync(long cmsCaseId)
        {
            var filter = $"caseId eq {cmsCaseId}";
            return await WaitForIndexCountResultsAsync(filter, 0);
        }

        public async Task<IList<StreamlinedSearchLine>> QueryAsync(long caseId, List<SearchFilterDocument> documents, string searchTerm)
        {
            var filter = GetCaseDocumentsSearchQuery(caseId, documents);
            var searchOptions = new SearchOptions
            {
                Filter = filter
            };

            // => e.g. search=caseId eq 2146928 and ((documentId eq '8660287' and versionId eq 7921776) or (documentId eq '8660286' and versionId eq 7921777) or (documentId eq '8660260' and versionId eq 7921740) or (documentId eq '8660255' and versionId eq 7921733) or (documentId eq '8660254' and versionId eq 7921732) or (documentId eq '8660253' and versionId eq 7921731) or (documentId eq '8660252' and versionId eq 7921730) or (documentId eq 'PCD-131307' and versionId eq 1) or (documentId eq 'DAC' and versionId eq 1))
            var searchResults = await _azureSearchClient.SearchAsync<SearchLine>(searchTerm, searchOptions);
            var searchLines = new List<SearchLine>();
            await foreach (var searchResult in searchResults.Value.GetResultsAsync())
            {
                searchLines.Add(searchResult.Document);
            }

            return BuildStreamlinedResults(searchLines, searchTerm);
        }

        public IList<StreamlinedSearchLine> BuildStreamlinedResults(IList<SearchLine> searchResults, string searchTerm)
        {
            var streamlinedResults = new List<StreamlinedSearchLine>();
            if (searchResults.Count == 0)
            {
                return streamlinedResults;
            }

            var searchResultsValues
                = searchResults.Select(searchResult => _streamlinedSearchResultFactory.Create(searchResult, searchTerm));

            streamlinedResults.AddRange(searchResultsValues);

            return streamlinedResults;
        }

        public async Task<IndexDocumentsDeletedResult> RemoveCaseIndexEntriesAsync(long caseId)
        {
            if (caseId == 0)
            {
                throw new ArgumentException("Invalid caseId", nameof(caseId));
            }

            var searchOptions = new SearchOptions { Filter = $"caseId eq {caseId}" };

            var results = await _azureSearchClient.SearchAsync<SearchLine>("*", searchOptions);
            var searchLines = new List<SearchLine>();
            await foreach (var searchResult in results.Value.GetResultsAsync())
            {
                searchLines.Add(searchResult.Document);
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

        private async Task<IndexSettledResult> WaitForIndexCountResultsAsync(string filter, long targetCount)
        {
            var options = new SearchOptions
            {
                Filter = filter,
                Size = 0,
                IncludeTotalCount = true,
            };

            var baseDelayMs = IndexSettleUnitDelayMs;
            var recordCounts = new List<long>();

            foreach (var timeoutBase in Fibonacci(IndexSettleRetryAttemptCount))
            {
                var searchResults = await _azureSearchClient.SearchAsync<SearchLine>("*", options);
                var receivedLinesCount = searchResults.Value.TotalCount;
                recordCounts.Add(receivedLinesCount ?? -1);

                if (receivedLinesCount == targetCount)
                {
                    break;
                }

                var timeout = baseDelayMs * timeoutBase;
                await Task.Delay(timeout);
            }

            return new IndexSettledResult
            {
                TargetCount = targetCount,
                IsSuccess = recordCounts.Any() && recordCounts.Last() == targetCount,
                RecordCounts = recordCounts
            };
        }

        private string GetCaseDocumentsSearchQuery(long caseId, List<SearchFilterDocument> documents)
        {
            var stringBuilder = new StringBuilder($"caseId eq {caseId}");

            if (documents.Any())
            {
                stringBuilder.Append(" and (");
                stringBuilder.Append($@"(documentId eq '{documents[0].CmsDocumentId}' and versionId eq {documents[0].CmsVersionId})");
                for (var i = 1; i < documents.Count; i++)
                {
                    stringBuilder.Append(@$" or (documentId eq '{documents[i].CmsDocumentId}' and versionId eq {documents[i].CmsVersionId})");
                }
                stringBuilder.Append(")");
            }
            return stringBuilder.ToString();
        }

        private IEnumerable<int> Fibonacci(int n)
        {
            int prev = 0, current = 1;
            for (int i = 0; i < n; i++)
            {
                yield return current;
                int temp = prev;
                prev = current;
                current = temp + current;
            }
        }
    }
}
