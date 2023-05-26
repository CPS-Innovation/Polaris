using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents;
using Common.Domain.SearchIndex;
using Common.Factories.Contracts;
using Common.Logging;
using Common.Services.SearchIndexService.Contracts;
using Common.ValueObjects;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;

namespace Common.Services.SearchIndexService
{
    [ExcludeFromCodeCoverage]
    public class SearchIndexService : ISearchIndexService
    {
        private readonly SearchClient _searchClient;
        private readonly ISearchLineFactory _searchLineFactory;
        private readonly ISearchIndexingBufferedSenderFactory _searchIndexingBufferedSenderFactory;
        private readonly ILogger<SearchIndexService> _logger;

        public SearchIndexService(
            ISearchClientFactory searchClientFactory,
            ISearchLineFactory searchLineFactory,
            ISearchIndexingBufferedSenderFactory searchIndexingBufferedSenderFactory,
            ILogger<SearchIndexService> logger)
        {
            _searchClient = searchClientFactory.Create();
            _searchLineFactory = searchLineFactory;
            _searchIndexingBufferedSenderFactory = searchIndexingBufferedSenderFactory;
            _logger = logger;
        }

        public async Task StoreResultsAsync(AnalyzeResults analyzeResults, PolarisDocumentId polarisDocumentId, long cmsCaseId, string cmsDocumentId, long versionId, string blobPath, Guid correlationId)
        {
            string blobName = Path.GetFileName(blobPath);
            _logger.LogMethodEntry(correlationId, nameof(StoreResultsAsync), $"PolarisDocumentId: {polarisDocumentId}, CmsCaseId: {cmsCaseId}, Blob Name: {blobName}");
            
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
                await using var indexer = _searchIndexingBufferedSenderFactory.Create(_searchClient);

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
                _logger.LogMethodFlow(correlationId, nameof(StoreResultsAsync),
                    $"Updating the search index completed - number of lines: {lines.Count}, successes: {successCount}, failures: {failureCount}");

                if (!await indexTaskCompletionSource.Task)
                {
                    throw new RequestFailedException($"At least one indexing action failed. Status(es) = {string.Join(", ", statuses)}");
                }
            }
            else
            {
                _logger.LogMethodFlow(correlationId, nameof(StoreResultsAsync),
                    "No OCR results generated for this document, therefore no need to update the search index... returning...");
            }
        }
        
        public async Task RemoveResultsByBlobNameAsync(long caseId, string blobName, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(RemoveResultsByBlobNameAsync), $"CaseId: {caseId}, BlobName: {blobName}");

            if (caseId == 0)
                throw new ArgumentException("Invalid caseId", nameof(caseId));

            if (string.IsNullOrWhiteSpace(blobName))
                throw new ArgumentException("Invalid Blob Name", nameof(blobName));

            var searchOptions = new SearchOptions
                {
                    Filter = $"caseId eq {caseId} and blobName eq '{blobName}'"
                };
            
            var results = await _searchClient.SearchAsync<SearchLine>("*", searchOptions);
            var searchLines = new List<SearchLine>();
            await foreach (var searchResult in results.Value.GetResultsAsync())
            {
                searchLines.Add(searchResult.Document);
            }

            if (searchLines.Count == 0)
            {
                _logger.LogMethodFlow(correlationId, nameof(RemoveResultsByBlobNameAsync), 
                    "No results found - all documents for this case have been previously removed");
            }
            else
            {
                await using var indexer = _searchIndexingBufferedSenderFactory.Create(_searchClient);
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
                _logger.LogMethodFlow(correlationId, nameof(RemoveResultsByBlobNameAsync),
                $"Updating the search index completed following a deletion request for caseId '{caseId}' and blobName '{blobName}' " +
                            $"- number of lines: {searchLines.Count}, successes: {successCount}, failures: {failureCount}");

                if (!await indexTaskCompletionSource.Task)
                {
                    throw new RequestFailedException("At least one indexing action failed.");
                }
            }
        }
    }
}
