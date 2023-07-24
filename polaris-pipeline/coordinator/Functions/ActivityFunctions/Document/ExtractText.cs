using System.Threading.Tasks;
using Common.Clients.Contracts;
using Common.Logging;
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
        private readonly ILogger _logger;

        public ExtractText(IPolarisBlobStorageService blobStorageService,
            ITextExtractorClient textExtractorClient,
            ILogger<GeneratePdf> logger)
        {
            _blobStorageService = blobStorageService;
            _textExtractorClient = textExtractorClient;
            _logger = logger;   
        }

        [FunctionName(nameof(ExtractText))]
        public async Task Run([ActivityTrigger] IDurableActivityContext context)
        {
            var payload = context.GetInput<CaseDocumentOrchestrationPayload>();

            var documentStream = await _blobStorageService.GetDocumentAsync(payload.BlobName, payload.CorrelationId);
            if(documentStream != null)
                _logger.LogFileStream($"BlobStorage-GetDocument-{nameof(ExtractText)}", $"{payload.CmsCaseId}-{payload.CmsDocumentId}-{payload.CmsVersionId}-{payload.BlobName.Replace("/", "-")}", "PDF", documentStream);

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