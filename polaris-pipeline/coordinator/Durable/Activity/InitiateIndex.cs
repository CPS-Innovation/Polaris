using System.Threading.Tasks;
using Common.Dto.Response;
using Common.Services.BlobStorage;
using coordinator.Clients.TextExtractor;
using coordinator.Durable.Payloads;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Durable.Activity
{
    public class InitiateIndex
    {
        private readonly IPolarisBlobStorageService _polarisBlobStorageService;
        private readonly ITextExtractorClient _textExtractorClient;

        public InitiateIndex(IPolarisBlobStorageService blobStorageService, ITextExtractorClient textExtractorClient)
        {
            _polarisBlobStorageService = blobStorageService;
            _textExtractorClient = textExtractorClient;
        }

        [FunctionName(nameof(InitiateIndex))]
        public async Task<StoreCaseIndexesResult> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var payload = context.GetInput<DocumentPayload>();
            var blobId = new BlobIdType(payload.CaseId, payload.DocumentId, payload.VersionId, BlobType.Ocr);

            using var documentStream = await _polarisBlobStorageService.GetBlobAsync(blobId);
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