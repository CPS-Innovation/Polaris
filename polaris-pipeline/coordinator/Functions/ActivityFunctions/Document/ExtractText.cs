using System.Threading.Tasks;
using Common.Clients.Contracts;
using Common.Services.BlobStorageService.Contracts;
using coordinator.Domain;
using coordinator.Functions.ActivityFunctions.Document;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.Orchestration.Functions.Document
{
    public class ExtractText
    {
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly ITextExtractorClient _textExtractorClient;

        public ExtractText(IPolarisBlobStorageService blobStorageService,
            ITextExtractorClient textExtractorClient,
            ILogger<GeneratePdf> logger)
        {
            _blobStorageService = blobStorageService;
            _textExtractorClient = textExtractorClient;
        }

        [FunctionName(nameof(ExtractText))]
        public async Task Run([ActivityTrigger] IDurableActivityContext context)
        {
            var payload = context.GetInput<CaseDocumentOrchestrationPayload>();

            var documentStream = await _blobStorageService.GetDocumentAsync(payload.BlobName, payload.CorrelationId);

            await _textExtractorClient.ExtractTextAsync(payload.PolarisDocumentId,
                payload.CmsCaseId,
                payload.CmsDocumentId,
                payload.CmsVersionId,
                payload.BlobName,
                payload.CorrelationId,
                documentStream);
        }
    }
}