using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Common.Logging;
using Common.Services.SearchIndexService.Contracts;
using Common.ValueObjects;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;

namespace Common.Services.SearchIndexService
{
    [ExcludeFromCodeCoverage]
    public class MockSearchIndexService : ISearchIndexService
    {
        private readonly ILogger<MockSearchIndexService> _logger;

        public MockSearchIndexService(
            ILogger<MockSearchIndexService> logger)
        {
            _logger = logger;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task StoreResultsAsync(AnalyzeResults analyzeResults, PolarisDocumentId polarisDocumentId, long caseId, string documentId, long versionId, string blobName, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, $"{nameof(MockSearchIndexService)}.{nameof(StoreResultsAsync)}", $"PolarisDocumentId: {polarisDocumentId}, CaseId: {caseId}, Blob Name: {blobName}");
        }

        public async Task RemoveResultsByBlobNameAsync(long caseId, string blobName, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, $"{nameof(MockSearchIndexService)}.{nameof(RemoveResultsByBlobNameAsync)}", $"CaseId: {caseId}, BlobName: {blobName}");
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}
