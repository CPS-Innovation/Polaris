using System.Threading.Tasks;
using Common.Dto.Response;
using Common.Services.BlobStorageService.Contracts;
using coordinator.Clients.Contracts;
using coordinator.Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Functions.Orchestration.Functions.Document
{
    public class ExtractText
    {
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly ITextExtractorClient _textExtractorClient;

        public ExtractText(IPolarisBlobStorageService blobStorageService,
            ITextExtractorClient textExtractorClient)
        {
            _blobStorageService = blobStorageService;
            _textExtractorClient = textExtractorClient;
        }

        [FunctionName(nameof(ExtractText))]
        public async Task<ExtractTextResult> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var payload = context.GetInput<CaseDocumentOrchestrationPayload>();

            var documentStream = await _blobStorageService.GetDocumentAsync(payload.BlobName, payload.CorrelationId);

            return await _textExtractorClient.ExtractTextAsync(payload.PolarisDocumentId,
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