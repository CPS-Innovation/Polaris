using System.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common.Domain.Entity;
using Common.Domain.SearchIndex;
using Common.Factories.Contracts;
using Common.Services.CaseSearchService.Contracts;
using Common.ValueObjects;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.Cosmos;

namespace Common.Services.CaseSearchService
{
    public class CosmosDbSearchClient : ICaseSearchClient
    {
        private readonly ISearchLineFactory _searchLineFactory;
        private readonly IStreamlinedSearchResultFactory _streamlinedSearchResultFactory;
        public readonly CosmosClient _client;
        public CosmosDbSearchClient(
            CosmosClient client,
            ISearchLineFactory searchLineFactory,
            IStreamlinedSearchResultFactory streamlinedSearchResultFactory)
        {
            _client = client;
            _searchLineFactory = searchLineFactory;
            _streamlinedSearchResultFactory = streamlinedSearchResultFactory;
        }

        /*
            At the time of writing:
            
             - partition Id is the caseId
             - index Policy index definition on the container
            {
                "indexingMode": "consistent",
                "automatic": true,
                "includedPaths": [
                    {
                        "path": "/caseId/?"
                    },
                    {
                        "path": "/documentId/?"
                    },
                    {
                        "path": "/versionId/?"
                    },
                    {
                        "path": "/words/[]/text/?"
                    }
                ],
                "excludedPaths": [
                    {
                        "path": "/*"
                    },
                    {
                        "path": "/\"_etag\"/?"
                    }
                ]
            }
        */

        public async Task<IList<StreamlinedSearchLine>> QueryAsync(int caseId, List<BaseDocumentEntity> documents, string searchTerm, Guid correlationId)
        {
            var query = new StringBuilder($"SELECT * FROM c WHERE ARRAY_CONTAINS(c.words, {{\"text\":\"{searchTerm.ToLowerInvariant()}\" }}, true)");

            // query.Append("AND (1 = 0 ") ;
            // foreach (var document in documents)
            // {
            //     query.Append($" OR (c.documentId = '{document.CmsDocumentId}' AND c.versionId = {document.CmsVersionId})");
            // }
            // query.Append(")");

            var results = await QueryCosmosDbAsync<SearchLine>(query.ToString(), caseId);

            return results.Select(result => _streamlinedSearchResultFactory.Create(result, searchTerm, correlationId)).ToList();
        }

        public async Task RemoveCaseIndexEntriesAsync(long caseId, Guid correlationId)
        {
            await DeleteEntriesAsync("SELECT c._self FROM c", caseId);

            // can use a preview bit of functionality, but this is not yet enabled on our Azure subscriptions
            //await _container.DeleteAllItemsByPartitionKeyStreamAsync(new PartitionKey(caseId));
        }

        public Task RemoveResultsByBlobNameAsync(long cmsCaseId, string blobName, Guid correlationId)
        {
            // todo: if using cosmosdb long-term, use TTl on doc entries and this method is a noop, or can be removed
            return Task.CompletedTask;
        }

        public async Task SendStoreResultsAsync(AnalyzeResults analyzeResults, PolarisDocumentId polarisDocumentId, long cmsCaseId, string cmsDocumentId, long versionId, string blobPath, Guid correlationId)
        {
            string blobName = Path.GetFileName(blobPath);

            await DeleteEntriesAsync($"SELECT c._self FROM c WHERE c.documentId = '{cmsDocumentId}'", cmsCaseId);

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
                var sanitizedSearchLine = CosmosSpecificSanitization(searchLines);
                lines.AddRange(sanitizedSearchLine);
            }

            var tasks = new List<Task>(lines.Count);
            var container = getContainer();
            foreach (var searchLine in lines)
            {
                tasks.Add(container.CreateItemAsync(
                    searchLine,
                    new PartitionKey(searchLine.CaseId),
                    new ItemRequestOptions()
                    {
                        EnableContentResponseOnWrite = false
                    }));
            }

            await Task.WhenAll(tasks);
        }
        public async Task<bool> WaitForCaseEmptyResultsAsync(long cmsCaseId, Guid correlationId)
        {
            // Cosmos seems to be atomic, so just return true;
            return await Task.FromResult(true);

            // return await WaitForStoreResultsAsync(
            //     $"SELECT VALUE COUNT(1) FROM c",
            //     0,
            //     cmsCaseId);
        }

        public async Task<bool> WaitForStoreResultsAsync(AnalyzeResults analyzeResults, long cmsCaseId, string cmsDocumentId, long versionId, Guid correlationId)
        {
            // Cosmos seems to be atomic, so just return true;
            return await Task.FromResult(true);

            // return await WaitForStoreResultsAsync(
            //     $"SELECT VALUE COUNT(1) FROM c WHERE c.documentId = '{cmsDocumentId}' AND c.versionId = {versionId}",
            //     analyzeResults.ReadResults.Sum(r => r.Lines.Count),
            //     cmsCaseId);
        }

        private async Task<bool> WaitForStoreResultsAsync(string sqlQueryText, int expectedCount, long cmsCaseId)
        {
            foreach (var timeoutBase in Fibonacci(5))
            {
                var results = await QueryCosmosDbAsync<long>(sqlQueryText, cmsCaseId);
                if (results.First() == expectedCount)
                {
                    return true;
                }

                var baseDelayMs = 250;
                var timeout = baseDelayMs * timeoutBase;

                await Task.Delay(timeout);
            }
            return false;
        }

        private async Task DeleteEntriesAsync(string query, long caseId)
        {
            while (true)
            {
                // the stored procedure from here: https://github.com/Azure/azure-cosmosdb-js-server/blob/master/samples/stored-procedures/bulkDelete.js
                var response = await getContainer().Scripts.ExecuteStoredProcedureAsync<DeleteProcResultObject>(
                    "bulkDeleteSproc",
                    new PartitionKey(caseId),
                    new dynamic[] { query });

                if (response.Resource.Continuation)
                {
                    await Task.Delay(100);
                }
                else
                {
                    break;
                }
            }
        }

        private async Task<List<T>> QueryCosmosDbAsync<T>(string sqlQueryText, long cmsCaseId)
        {
            var queryDefinition = new QueryDefinition(sqlQueryText);

            var queryResultSetIterator = getContainer().GetItemQueryIterator<T>(
                queryDefinition,
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(cmsCaseId)
                });

            var results = new List<T>();
            while (queryResultSetIterator.HasMoreResults)
            {
                var response = await queryResultSetIterator.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
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

        private Container getContainer()
        {
            return _client.GetContainer("Search", "Items");
        }

        private IEnumerable<SearchLine> CosmosSpecificSanitization(IEnumerable<SearchLine> searchLines)
        {
            foreach (var searchLine in searchLines)
            {
                foreach (var word in searchLine.Words)
                {
                    var convertedWord = new string(word.Text.Where(c => !char.IsPunctuation(c)).ToArray())
                        .ToLowerInvariant();

                    if (convertedWord != word.Text)
                    {
                        word.Text = convertedWord;
                    }
                }
            }
            return searchLines;
        }

        private class DeleteProcResultObject
        {
            public int Deleted { get; set; }
            public bool Continuation { get; set; }
        }
    }
}
