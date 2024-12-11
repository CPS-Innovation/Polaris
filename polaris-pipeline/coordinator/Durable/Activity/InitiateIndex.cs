using System;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Dto.Response;
using Common.Services.BlobStorage;
using coordinator.Clients.TextExtractor;
using coordinator.Durable.Payloads;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;

namespace coordinator.Durable.Activity
{
    public class InitiateIndex
    {
        private readonly IPolarisBlobStorageService _polarisBlobStorageService;
        private readonly ITextExtractorClient _textExtractorClient;

        public InitiateIndex(Func<string, IPolarisBlobStorageService> blobStorageServiceFactory, ITextExtractorClient textExtractorClient, IConfiguration configuration)
        {
            _polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty) ?? throw new ArgumentNullException(nameof(blobStorageServiceFactory));
            _textExtractorClient = textExtractorClient;
        }

        [FunctionName(nameof(InitiateIndex))]
        public async Task<StoreCaseIndexesResult> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var payload = context.GetInput<DocumentPayload>();
            var blobId = new BlobIdType(payload.CaseId, payload.DocumentId, payload.VersionId, BlobType.Ocr);

            await using var documentStream = await _polarisBlobStorageService.GetBlobAsync(blobId);
            return await _textExtractorClient.StoreCaseIndexesAsync(
                payload.DocumentId,
                payload.Urn,
                payload.CaseId,
                payload.VersionId,
                payload.CorrelationId,
                documentStream);
        }
    }
}