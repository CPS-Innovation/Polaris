using System.Threading.Tasks;
using coordinator.Clients.Contracts;
using Common.Services.BlobStorageService.Contracts;
using coordinator.Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Functions.Orchestration.ActivityFunctions
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
        public async Task Run([ActivityTrigger] IDurableActivityContext context)
        {
            var payload = context.GetInput<CaseDocumentOrchestrationPayload>();

            using var documentStream = await _blobStorageService.GetDocumentAsync(payload.BlobName, payload.CorrelationId);

            await _textExtractorClient.ExtractTextAsync(payload.PolarisDocumentId,
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