using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Common.Dto.Response;
using Common.Services.BlobStorageService;
using Common.Telemetry;
using coordinator.Clients.TextExtractor;
using coordinator.Durable.Payloads;
using coordinator.Services.OcrService;
using coordinator.Services.TextExtractService;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using text_extractor.coordinator;

namespace coordinator.Durable.Activity
{
    public class ExtractText
    {
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly ITextExtractorClient _textExtractorClient;
        private readonly IOcrService _ocrService;
        private readonly ITextExtractService _textExtractService;
        private readonly ITelemetryClient _telemetryClient;

        public ExtractText(IPolarisBlobStorageService blobStorageService, ITextExtractorClient textExtractorClient, IOcrService ocrService,
            ITextExtractService textExtractService, ITelemetryClient telemetryClient)
        {
            _blobStorageService = blobStorageService;
            _textExtractorClient = textExtractorClient;
            _ocrService = ocrService;
            _textExtractService = textExtractService;
            _telemetryClient = telemetryClient;
        }

        [FunctionName(nameof(ExtractText))]
        public async Task<ExtractTextResult> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var extractTextResult = new ExtractTextResult();
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

                var ocrResults = await _ocrService.GetOcrResultsAsync(documentStream, payload.CorrelationId);
                var ocrLineCount = ocrResults.ReadResults.Sum(x => x.Lines.Count);

                extractTextResult = new ExtractTextResult()
                {
                    OcrCompletedTime = DateTime.UtcNow,
                    PageCount = ocrResults.ReadResults.Count,
                    LineCount = ocrLineCount,
                    WordCount = ocrResults.ReadResults.Sum(x => x.Lines.Sum(y => y.Words.Count)),
                    IsSuccess = true
                };

                telemetryEvent.OcrCompletedTime = extractTextResult.OcrCompletedTime;
                telemetryEvent.PageCount = extractTextResult.PageCount;
                telemetryEvent.LineCount = extractTextResult.LineCount;
                telemetryEvent.WordCount = extractTextResult.WordCount;

                var jsonResults = JsonSerializer.Serialize(ocrResults.ReadResults);
                var ocrBlobName = GetOcrBlobName(payload.BlobName);

                using (var ocrStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonResults)))
                {
                    await _blobStorageService.UploadDocumentAsync(
                        ocrStream,
                        ocrBlobName,
                        payload.CmsCaseId.ToString(),
                        payload.PolarisDocumentId,
                        payload.CmsVersionId.ToString(),
                        payload.CorrelationId);

                    telemetryEvent.OcrResultsStoredTime = DateTime.UtcNow;
                }

                using (var ocrStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonResults)))
                {
                    var storeCaseIndexesResult = await _textExtractorClient.StoreCaseIndexesAsync(
                        payload.PolarisDocumentId,
                        payload.CmsCaseUrn,
                        payload.CmsCaseId,
                        payload.CmsDocumentId,
                        payload.CmsVersionId,
                        ocrBlobName,
                        payload.CorrelationId,
                        ocrStream);

                    telemetryEvent.IndexStoredTime = storeCaseIndexesResult.IndexStoredTime;
                }

                await Task.Delay(1000);

                var searchIndexCountResult = await _textExtractService.WaitForDocumentStoreResultsAsync(
                    payload.CmsCaseUrn,
                    payload.CmsCaseId,
                    payload.CmsDocumentId,
                    payload.CmsVersionId,
                    ocrLineCount,
                    payload.CorrelationId
                );

                telemetryEvent.DidIndexSettle = searchIndexCountResult.IsSuccess;
                telemetryEvent.WaitRecordCounts = searchIndexCountResult.RecordCounts;
                telemetryEvent.IndexSettleTargetCount = searchIndexCountResult.TargetCount;

                telemetryEvent.EndTime = DateTime.UtcNow;
                _telemetryClient.TrackEvent(telemetryEvent);

                return extractTextResult;
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