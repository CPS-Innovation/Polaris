using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents;
using Common.Domain.Entity;
using Common.Domain.SearchIndex;
using Common.Factories.Contracts;
using Common.Logging;
using Common.Services.CaseSearchService.Contracts;
using Common.ValueObjects;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;

namespace Common.Services.CaseSearchService
{
    public class CaseSearchClient : ICaseSearchClient
    {
        private readonly SearchClient _azureSearchClient;
        private readonly ISearchLineFactory _searchLineFactory;
        private readonly ISearchIndexingBufferedSenderFactory _searchIndexingBufferedSenderFactory;
        private readonly IStreamlinedSearchResultFactory _streamlinedSearchResultFactory;
        private readonly ILogger<CaseSearchClient> _logger;

        public CaseSearchClient(
            IAzureSearchClientFactory searchClientFactory,
            ISearchLineFactory searchLineFactory,
            ISearchIndexingBufferedSenderFactory searchIndexingBufferedSenderFactory,
            IStreamlinedSearchResultFactory streamlinedSearchResultFactory, 
            ILogger<CaseSearchClient> logger)
        {
            _azureSearchClient = searchClientFactory.Create();
            _searchLineFactory = searchLineFactory;
            _searchIndexingBufferedSenderFactory = searchIndexingBufferedSenderFactory;
            _streamlinedSearchResultFactory = streamlinedSearchResultFactory;
            _logger = logger;
        }

        public async Task StoreResultsAsync(AnalyzeResults analyzeResults, PolarisDocumentId polarisDocumentId, long cmsCaseId, string cmsDocumentId, long versionId, string blobPath, Guid correlationId)
        {
            string blobName = Path.GetFileName(blobPath);
            _logger.LogMethodEntry(correlationId, nameof(StoreResultsAsync), $"PolarisDocumentId: {polarisDocumentId}, CmsCaseId: {cmsCaseId}, Blob Name: {blobName}");

            //await RemoveDocumentIndexEntriesAsync(cmsCaseId, cmsDocumentId, versionId, correlationId);

            _logger.LogMethodFlow(correlationId, nameof(StoreResultsAsync), "Building search line results");
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

            if (lines.Count > 0)
            {
                _logger.LogMethodFlow(correlationId, nameof(StoreResultsAsync), "Beginning search index update");
                await using var indexer = _searchIndexingBufferedSenderFactory.Create(_azureSearchClient);

                var indexTaskCompletionSource = new TaskCompletionSource<bool>();
                HashSet<int?> statuses = new HashSet<int?>();

                var failureCount = 0;
                indexer.ActionFailed += error =>
                {
                    if( error.Exception is RequestFailedException )
                    {
                        var status = ((RequestFailedException)error.Exception)?.Status;
                        if( status != null )
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

                _logger.LogMethodFlow(correlationId, nameof(StoreResultsAsync), $"Updating the search index completed - number of lines: {lines.Count}, successes: {successCount}, failures: {failureCount}");

                if (!await indexTaskCompletionSource.Task)
                {
                    throw new RequestFailedException($"At least one indexing action failed. Status(es) = {string.Join(", ", statuses)}");
                }
            }
            else
            {
                _logger.LogMethodFlow(correlationId, nameof(StoreResultsAsync), "No OCR results generated for this document, therefore no need to update the search index... returning...");
            }
        }

        public async Task<IList<StreamlinedSearchLine>> QueryAsync(int caseId, List<BaseDocumentEntity> documents, string searchTerm, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(QueryAsync), $"CaseId '{caseId}', searchTerm '{searchTerm}'");

            var filter = GetCaseDocumentsSearchQuery(caseId, documents);
            var searchOptions = new SearchOptions
            {
                Filter = filter
            };

            // => e.g. search=carol&filter=caseId eq 2146928 and (versionId eq 7921776 or versionId eq7921775 or versionId eq 7921740 or versionId eq 7921733 or versionId eq 7921732 or versionId eq 7921731 or versionId eq 7921730 or versionId eq 1 or versionId eq 1)
            var searchResults = await _azureSearchClient.SearchAsync<SearchLine>(searchTerm, searchOptions);
            var searchLines = new List<SearchLine>();
            await foreach (var searchResult in searchResults.Value.GetResultsAsync())
            {
                if (IsLiveDocumentResult(documents, searchResult.Document))
                {
                    searchLines.Add(searchResult.Document);
                }
            }

            _logger.LogMethodFlow(correlationId, nameof(QueryAsync), $"Found {searchLines.Count} results, building streamlined search results");
            var results = BuildStreamlinedResults(searchLines, searchTerm, correlationId);
            _logger.LogMethodExit(correlationId, nameof(QueryAsync), string.Empty);
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

        public async Task RemoveCaseIndexEntriesAsync(long caseId, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(RemoveCaseIndexEntriesAsync), $"CaseId: {caseId}");

            #region Validate-Inputs
            if (caseId == 0)
                throw new ArgumentException("Invalid caseId", nameof(caseId));
            #endregion

            string filter = $"caseId eq {caseId}";

            await RemoveIndexEntries(filter, correlationId);

            _logger.LogMethodFlow
                (
                    correlationId,
                    nameof(RemoveCaseIndexEntriesAsync),
                    $"Updating the search index completed following a deletion request for caseId '{caseId}'"
                );
        }

        public async Task RemoveDocumentIndexEntriesAsync(long caseId, string documentId, long versionId, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(RemoveDocumentIndexEntriesAsync), $"CaseId: {caseId}, DoocumentId: {documentId}, Versionid {versionId}");

            #region Validate-Inputs
            if (caseId == 0)
                throw new ArgumentException("Invalid caseId", nameof(caseId));

            if (string.IsNullOrWhiteSpace(documentId))
                throw new ArgumentException("Invalid Document ID", nameof(documentId));
            #endregion

            string filter = $"caseId eq {caseId} and documentId eq '{documentId}' and versionId eq {versionId}";

            await RemoveIndexEntries(filter, correlationId);

            _logger.LogMethodFlow
                (
                    correlationId, 
                    nameof(RemoveDocumentIndexEntriesAsync),
                    $"Updating the search index completed following a deletion request for caseId '{caseId}', documentId '{documentId}', versionid '{versionId}'"
                );
        }

        public async Task RemoveResultsByBlobNameAsync(long caseId, string blobName, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(RemoveResultsByBlobNameAsync), $"CaseId: {caseId}, BlobName: {blobName}");

            if (caseId == 0)
                throw new ArgumentException("Invalid caseId", nameof(caseId));

            if (string.IsNullOrWhiteSpace(blobName))
                throw new ArgumentException("Invalid Blob Name", nameof(blobName));

            string filter = $"caseId eq {caseId} and blobName eq '{blobName}'";

            await RemoveIndexEntries(filter, correlationId);

            _logger.LogMethodFlow
                (
                    correlationId,
                    nameof(RemoveResultsByBlobNameAsync),
                    $"Updating the search index completed following a deletion request for caseId '{caseId}' and blobName '{blobName}'"
                );
        }

        private async Task RemoveIndexEntries(string filter, Guid correlationId)
        {
            var searchOptions = new SearchOptions { Filter = filter };

            var results = await _azureSearchClient.SearchAsync<SearchLine>("*", searchOptions);
            var searchLines = new List<SearchLine>();
            await foreach (var searchResult in results.Value.GetResultsAsync())
            {
                searchLines.Add(searchResult.Document);
            }

            if (searchLines.Count == 0)
            {
                _logger.LogMethodFlow(correlationId, nameof(RemoveDocumentIndexEntriesAsync), "No results found - this document has been previously removed");
            }
            else
            {
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
                _logger.LogMethodFlow
                    (
                        correlationId, 
                        nameof(RemoveIndexEntries),
                        $"number of lines: {searchLines.Count}, successes: {successCount}, failures: {failureCount}"
                    );

                if (!await indexTaskCompletionSource.Task)
                {
                    throw new RequestFailedException("At least one indexing action failed.");
                }
            }
        }

        private string GetCaseDocumentsSearchQuery(int caseId, List<BaseDocumentEntity> documents)
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
