using System;
using System.Text;
using System.Threading.Tasks;
using Common.Services.BlobStorageService;
using Common.Telemetry;
using coordinator.Durable.Payloads;
using coordinator.Services.OcrService;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using text_extractor.coordinator;

namespace coordinator.Durable.Activity
{
    public class InitiateOcr
    {
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly IOcrService _ocrService;
        private readonly ITelemetryClient _telemetryClient;

        public InitiateOcr(IPolarisBlobStorageService blobStorageService, IOcrService ocrService, ITelemetryClient telemetryClient)
        {
            _blobStorageService = blobStorageService;
            _ocrService = ocrService;
            _telemetryClient = telemetryClient;
        }

        [FunctionName(nameof(InitiateOcr))]
        public async Task<Guid> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var payload = context.GetInput<CaseDocumentOrchestrationPayload>();
            var telemetryEvent = new IndexedDocumentEvent(payload.CorrelationId)
            {
                CaseUrn = payload.CmsCaseUrn,
                CaseId = payload.CmsCaseId,
                DocumentId = payload.CmsDocumentId,
                DocumentTypeId = payload.DocumentTypeId,
                DocumentType = payload.DocumentType,
                DocumentCategory = payload.DocumentCategory,
                VersionId = payload.CmsVersionId,
                StartTime = DateTime.Now
            };

            try
            {
                using var documentStream = await _blobStorageService.GetDocumentAsync(payload.BlobName, payload.CorrelationId);
                return await _ocrService.InitiateOperationAsync(documentStream, payload.CorrelationId);
            }
            catch (Exception)
            {
                _telemetryClient.TrackEventFailure(telemetryEvent);
                throw;
            }
        }

        private string GetOcrBlobName(string blobName)
        {
            var stringBuilder = new StringBuilder(blobName);
            stringBuilder.Replace("/pdfs/", "/ocrs/");
            stringBuilder.Replace(".pdf", ".json");

            return stringBuilder.ToString();
        }
    }
}