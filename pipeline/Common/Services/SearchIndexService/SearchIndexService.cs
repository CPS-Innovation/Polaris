﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents;
using Common.Domain.SearchIndex;
using Common.Factories.Contracts;
using Common.Logging;
using Common.Services.SearchIndexService.Contracts;
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

        public async Task StoreResultsAsync(AnalyzeResults analyzeResults, long caseId, string documentId, long versionId, string blobName, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(StoreResultsAsync), $"CaseId: {caseId}, Blob Name: {blobName}");
            
            _logger.LogMethodFlow(correlationId, nameof(StoreResultsAsync), "Building search line results");
            var lines = new List<SearchLine>();
            foreach (var readResult in analyzeResults.ReadResults)
            {
                lines.AddRange(readResult.Lines.Select((line, index) => _searchLineFactory.Create(caseId, documentId, versionId, blobName, readResult, line, index)));
            }

            _logger.LogMethodFlow(correlationId, nameof(StoreResultsAsync), "Beginning search index update");
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
                throw new RequestFailedException("At least one indexing action failed.");
            }
        }
        
        public async Task RemoveResultsByDocumentVersionAsync(long caseId, string documentId, long versionId, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(RemoveResultsByDocumentVersionAsync), $"CaseId: {caseId}, DocumentId: {documentId}, VersionId: {versionId}");

            if (caseId == 0)
                throw new ArgumentException("Invalid caseId", nameof(caseId));

            if (string.IsNullOrWhiteSpace(documentId))
                throw new ArgumentException("Invalid documentId", nameof(documentId));

            var searchOptions = new SearchOptions
                {
                    Filter = $"caseId eq {caseId} and documentId eq '{documentId}' and versionId eq {versionId}"
                };
            
            var results = await _searchClient.SearchAsync<SearchLine>("*", searchOptions);
            var searchLines = new List<SearchLine>();
            await foreach (var searchResult in results.Value.GetResultsAsync())
            {
                searchLines.Add(searchResult.Document);
            }

            if (searchLines.Count == 0)
            {
                _logger.LogMethodFlow(correlationId, nameof(RemoveResultsByDocumentVersionAsync), 
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
                _logger.LogMethodFlow(correlationId, nameof(RemoveResultsByDocumentVersionAsync),
                $"Updating the search index completed following a deletion request for caseId '{caseId}' and documentId '{documentId}' " +
                            $"and versionId '{versionId}' - number of lines: {searchLines.Count}, successes: {successCount}, failures: {failureCount}");

                if (!await indexTaskCompletionSource.Task)
                {
                    throw new RequestFailedException("At least one indexing action failed.");
                }
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
