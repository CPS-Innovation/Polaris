using System.Threading.Tasks;
using Common.Dto.Response;
using Common.Services.BlobStorageService;
using coordinator.Clients.TextExtractor;
using coordinator.Durable.Payloads;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Durable.Activity.ExtractTextNext
{
    public class StoreIndex
    {
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly ITextExtractorClient _textExtractorClient;

        public StoreIndex(IPolarisBlobStorageService blobStorageService, ITextExtractorClient textExtractorClient)
        {
            _blobStorageService = blobStorageService;
            _textExtractorClient = textExtractorClient;
        }

        [FunctionName(nameof(StoreIndex))]
        public async Task<StoreCaseIndexesResult> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var payload = context.GetInput<CaseDocumentOrchestrationPayload>();
            using var documentStream = await _blobStorageService.GetDocumentAsync(payload.OcrBlobName, payload.CorrelationId);

            return await _textExtractorClient.StoreCaseIndexesAsync(
                payload.PolarisDocumentId,
                payload.CmsCaseUrn,
                payload.CmsCaseId,
                payload.CmsDocumentId,
                payload.CmsVersionId,
                payload.BlobName,
                payload.CorrelationId,
                documentStream);
        }
    }
}