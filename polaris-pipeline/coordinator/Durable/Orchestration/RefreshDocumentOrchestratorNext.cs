using System;
using System.Threading.Tasks;
using Common.Constants;
using Common.Dto.Response;
using Common.Logging;
using Common.Telemetry;
using Common.ValueObjects;
using coordinator.Durable.Activity;
using coordinator.Durable.Activity.ExtractTextNext;
using coordinator.Durable.Payloads;
using coordinator.Durable.Payloads.Domain;
using coordinator.Services.OcrService.Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using text_extractor.coordinator;

namespace coordinator.Durable.Orchestration
{
    public class RefreshDocumentOrchestratorNext : BaseOrchestrator
    {
        private readonly ILogger<RefreshDocumentOrchestratorNext> _log;
        private readonly ITelemetryClient _telemetryClient;
        const int _pollingIntervalMs = 1500;

        public static string GetKey(long caseId, PolarisDocumentId polarisDocumentId)
        {
            return $"[{caseId}]-{polarisDocumentId}";
        }

        public RefreshDocumentOrchestratorNext(ILogger<RefreshDocumentOrchestratorNext> log, ITelemetryClient telemetryClient)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        [FunctionName(nameof(RefreshDocumentOrchestratorNext))]
        public async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var payload = context.GetInput<CaseDocumentOrchestrationPayload>();
            var log = context.CreateReplaySafeLogger(_log);
            var caseEntity = CreateOrGetCaseDurableEntity(context, payload.CmsCaseId);

            // 1. Get Pdf
            try
            {
                var pdfConversionStatus = await context.CallActivityAsync<PdfConversionStatus>(nameof(GeneratePdf), payload);
                if (pdfConversionStatus != PdfConversionStatus.DocumentConverted)
                {
                    caseEntity.SetDocumentPdfConversionFailed((payload.PolarisDocumentId.ToString(), pdfConversionStatus));
                    return;
                }
            }
            catch (Exception exception)
            {
                caseEntity.SetDocumentPdfConversionFailed((payload.PolarisDocumentId.ToString(), PdfConversionStatus.UnexpectedError));
                log.LogMethodError(payload.CorrelationId, nameof(RefreshDocumentOrchestratorNext), $"Error calling {nameof(RefreshDocumentOrchestratorNext)}: {exception.Message}", exception);
                return;
            }

            // todo: this is temporary code until the coordinator refactor exercise is done.

            if (payload.DocumentDeltaType != DocumentDeltaType.RequiresIndexing)
            {
                // return and DO NOT set to PdfUploadedToBlob.  If we are refreshing the PDF it is because thr OCR flag has changed.
                //  The document will already either be at PdfUploadedToBlob or Indexed status.  If it is at Indexed status then we do not want to set the
                //  the flag back to PdfUploadedToBlob as Indexed is still correct.  As per comment above, all of this is to be rebuilt in pipeline refresh.
                return;
            }

            caseEntity.SetDocumentPdfConversionSucceeded((payload.PolarisDocumentId.ToString(), payload.BlobName));

            var telemetryEvent = new IndexedDocumentEvent(payload.CorrelationId)
            {
                CaseUrn = payload.CmsCaseUrn,
                CaseId = payload.CmsCaseId,
                DocumentId = payload.CmsDocumentId,
                DocumentTypeId = payload.DocumentTypeId,
                DocumentType = payload.DocumentType,
                DocumentCategory = payload.DocumentCategory,
                VersionId = payload.CmsVersionId,
                StartTime = context.CurrentUtcDateTime
            };

            try
            {
                // we use CallActivityWithRetryAsync for the OCR service interaction as it has ben seen to hang in production when making Http requests.
                //  The service will cancel and timeout at N seconds and throw.  So lets use the frameworks own retry mechanism.
                //  (During the great OCR meltdown incident at Microsoft, a MS engineer told us it was OK to just abandon operations within reasonable limits)
                var retryOptions = new RetryOptions(
                    firstRetryInterval: TimeSpan.FromSeconds(5),
                    maxNumberOfAttempts: 3);

                var ocrOperationId = await context.CallActivityWithRetryAsync<Guid>(
                    nameof(InitiateOcr),
                    retryOptions,
                    (payload.BlobName, payload.CorrelationId, payload.SubCorrelationId));

                var (_, ocrResults) = await PollingHelper.PollActivityUntilComplete<AnalyzeResults>(
                    context,
                    PollingHelper.CreatePollingArgs(nameof(CompleteOcr), _pollingIntervalMs, (ocrOperationId, payload.OcrBlobName, payload.CorrelationId, payload.SubCorrelationId)),
                    retryOptions);

                telemetryEvent.OcrCompletedTime = context.CurrentUtcDateTime;
                telemetryEvent.PageCount = ocrResults.PageCount;
                telemetryEvent.LineCount = ocrResults.LineCount;
                telemetryEvent.WordCount = ocrResults.WordCount;

                var indexStoredResult = await context.CallActivityAsync<StoreCaseIndexesResult>(nameof(InitiateIndex), payload);
                telemetryEvent.IndexStoredTime = context.CurrentUtcDateTime;

                var (waitRecordCounts, _) = await PollingHelper.PollActivityUntilComplete<long>(
                    context,
                    PollingHelper.CreatePollingArgs(nameof(CompleteIndex), _pollingIntervalMs, (payload, indexStoredResult.LineCount)));

                telemetryEvent.DidIndexSettle = true;
                telemetryEvent.WaitRecordCounts = waitRecordCounts;
                telemetryEvent.IndexSettleTargetCount = indexStoredResult.LineCount;
                telemetryEvent.EndTime = context.CurrentUtcDateTime;

                caseEntity.SetDocumentIndexingSucceeded(payload.PolarisDocumentId.ToString());

                // by this point we may be replaying, so good to keep a record
                telemetryEvent.DidOrchestratorReplay = context.IsReplaying;

                // assumption here that this will not be called multiple times as a result of replaying
                //  as there should be no replaying after we've got to this point (and completed the orchestration)
                _telemetryClient.TrackEvent(telemetryEvent);
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackEventFailure(telemetryEvent);

                caseEntity.SetDocumentIndexingFailed(payload.PolarisDocumentId.ToString());
                log.LogMethodError(payload.CorrelationId, nameof(RefreshDocumentOrchestratorNext), $"Error when running {nameof(RefreshDocumentOrchestratorNext)} orchestration: {exception.Message}", exception);
                return;
            }
        }
    }
}
