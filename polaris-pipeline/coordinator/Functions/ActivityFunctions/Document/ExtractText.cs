using System;
using System.Threading.Tasks;
using Common.Dto.Response;
using Common.Services.BlobStorageService.Contracts;
using Common.Telemetry.Contracts;
using coordinator.Clients.Contracts;
using coordinator.Domain;
using coordinator.Services.TextExtractService;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using text_extractor.coordinator;

namespace coordinator.Functions.Orchestration.Functions.Document
{
    public class ExtractText
    {
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly ITextExtractorClient _textExtractorClient;
        private readonly ITextExtractService _textExtractService;
        private readonly ITelemetryClient _telemetryClient;

        public ExtractText(IPolarisBlobStorageService blobStorageService, ITextExtractorClient textExtractorClient,
            ITextExtractService textExtractService, ITelemetryClient telemetryClient)
        {
            _blobStorageService = blobStorageService;
            _textExtractorClient = textExtractorClient;
            _textExtractService = textExtractService;
            _telemetryClient = telemetryClient;
        }

        [FunctionName(nameof(ExtractText))]
        public async Task<ExtractTextResult> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var payload = context.GetInput<CaseDocumentOrchestrationPayload>();

            var telemetryEvent = new IndexedDocumentEvent(payload.CorrelationId)
            {
                CaseUrn = payload.CmsCaseUrn,
                CaseId = payload.CmsCaseId,
                DocumentId = payload.CmsDocumentId,
                VersionId = payload.CmsVersionId,
                StartTime = DateTime.Now
            };

            try
            {
                using var documentStream = await _blobStorageService.GetDocumentAsync(payload.BlobName, payload.CorrelationId);

                var extractTextResult = await _textExtractorClient.ExtractTextAsync(payload.PolarisDocumentId,
                    payload.CmsCaseUrn,
                    payload.CmsCaseId,
                    payload.CmsDocumentId,
                    payload.CmsVersionId,
                    payload.DocumentTypeId,
                    payload.DocumentType,
                    payload.DocumentCategory,
                    payload.BlobName,
                    payload.CorrelationId,
                    documentStream);

                telemetryEvent.OcrCompletedTime = extractTextResult.OcrCompletedTime;
                telemetryEvent.PageCount = extractTextResult.PageCount;
                telemetryEvent.LineCount = extractTextResult.LineCount;
                telemetryEvent.WordCount = extractTextResult.WordCount;

                await Task.Delay(1000);

                var searchIndexCountResult = await _textExtractService.WaitForDocumentStoreResultsAsync(
                    payload.CmsCaseUrn,
                    payload.CmsCaseId,
                    payload.CmsDocumentId,
                    payload.CmsVersionId,
                    extractTextResult.LineCount,
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
    }
}