using System;
using System.Threading.Tasks;
using Common.Constants;
using Common.Domain.Document;
using Common.Domain.Ocr;
using Common.Dto.Response;
using Common.Logging;
using Common.Telemetry;
using coordinator.Durable.Activity;
using coordinator.Durable.Entity;
using coordinator.Durable.Payloads;
using coordinator.Durable.Payloads.Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using text_extractor.coordinator;

namespace coordinator.Durable.Orchestration
{
    public class RefreshDocumentOrchestrator
    {
        private readonly ILogger<RefreshDocumentOrchestrator> _log;
        private readonly ITelemetryClient _telemetryClient;
        private const int _prePollingDelayMs = 3000;
        private const int _pollingIntervalMs = 3000;
        private const int _maxPollingAttempts = 6;

        // Here we use CallActivityWithRetryAsync for the OCR service interaction as it has ben seen to hang in production when making Http requests.
        //  The service will cancel and timeout at N seconds and throw.  So lets use the durable framework's own retry mechanism.
        //  (During the great OCR meltdown incident at Microsoft, a MS engineer told us it was OK to just abandon operations within reasonable limits)
        private readonly RetryOptions _durableActivityRetryOptions = new RetryOptions(
            firstRetryInterval: TimeSpan.FromSeconds(5),
            maxNumberOfAttempts: 3
        );

        public static string GetKey(int caseId, string documentId)
        {
            return $"[{caseId}]-{documentId}";
        }

        public RefreshDocumentOrchestrator(ILogger<RefreshDocumentOrchestrator> log, ITelemetryClient telemetryClient)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        [FunctionName(nameof(RefreshDocumentOrchestrator))]
        public async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var payload = context.GetInput<DocumentPayload>();
            var log = context.CreateReplaySafeLogger(_log);
            var caseEntity = context.CreateEntityProxy<ICaseDurableEntity>(
                 CaseDurableEntity.GetEntityId(payload.CaseId)
            );

            var shouldProceed = await EnsurePdfExists(payload, context, caseEntity, log);
            if (!shouldProceed)
            {
                return;
            }

            // todo: this is temporary code until the coordinator refactor exercise is done.
            if (payload.DocumentDeltaType != DocumentDeltaType.RequiresIndexing)
            {
                // return and DO NOT call SetDocumentPdfConversionSucceeded.  If we are refreshing the PDF it is because thr OCR flag has changed.
                //  The document will already either be at PdfUploadedToBlob or Indexed status.  If it is at Indexed status then we do not want to set the
                //  the flag back to PdfUploadedToBlob as Indexed is still correct.  As per comment above, all of this is to be rebuilt in pipeline refresh.
                return;
            }

            //caseEntity.SetDocumentPdfConversionSucceeded(payload.DocumentId);

            var telemetryEvent = new IndexedDocumentEvent(payload.CorrelationId)
            {
                CaseUrn = payload.Urn,
                CaseId = payload.CaseId,
                DocumentId = payload.DocumentId,
                // DocumentTypeId = payload.DocumentType.DocumentTypeId,
                // DocumentType = payload.DocumentType.DocumentType,
                // DocumentCategory = payload.DocumentType.DocumentCategory,
                VersionId = payload.VersionId,
                StartTime = context.CurrentUtcDateTime
            };

            // 2 START
            try
            {
                var (ocrBlobAlreadyExists, ocrPollingResult) = await EnsureOcrExists(payload, context, log);

                telemetryEvent.OcrCompletedTime = context.CurrentUtcDateTime;
                telemetryEvent.PageCount = ocrPollingResult.FinalResult.PageCount;
                telemetryEvent.LineCount = ocrPollingResult.FinalResult.LineCount;
                telemetryEvent.WordCount = ocrPollingResult.FinalResult.WordCount;

                var (indexStoredTime, indexStoredResult, indexPollingResult) = await IndexDocument(context, payload, log, telemetryEvent);

                telemetryEvent.IndexStoredTime = indexStoredTime;
                telemetryEvent.DidIndexSettle = indexPollingResult.IsCompleted;
                telemetryEvent.WaitRecordCounts = indexPollingResult.Results;
                telemetryEvent.IndexSettleTargetCount = indexStoredResult.LineCount;
                telemetryEvent.EndTime = context.CurrentUtcDateTime;

                caseEntity.SetDocumentIndexingSucceeded(payload.DocumentId.ToString());

                // by this point we may be replaying, so good to keep a record
                telemetryEvent.DidOrchestratorReplay = context.IsReplaying;

                // assumption here that this will not be called multiple times as a result of replaying
                //  as there should be no replaying after we've got to this point (and completed the orchestration)
                _telemetryClient.TrackEvent(telemetryEvent);
            }
            catch (Exception exception)
            {
                // todo: there is no durable replay protection here, and there is evidence of several failure event records for the same failure event in our analytics.
                _telemetryClient.TrackEventFailure(telemetryEvent);

                caseEntity.SetDocumentIndexingFailed(payload.DocumentId.ToString());
                log.LogMethodError(payload.CorrelationId, nameof(RefreshDocumentOrchestrator), $"Error when running {nameof(RefreshDocumentOrchestrator)} orchestration: {exception.Message}", exception);
                return;
            }
        }

        private async Task<bool> EnsurePdfExists(DocumentPayload payload, IDurableOrchestrationContext context, ICaseDurableEntity caseEntity, ILogger log)
        {
            try
            {
                var activityName = payload.DocumentNatureType switch
                {
                    DocumentNature.Types.PreChargeDecisionRequest => nameof(GeneratePdfFromPcdRequest),
                    DocumentNature.Types.DefendantsAndCharges => nameof(GeneratePdfFromDefendantsAndCharges),
                    _ => nameof(GeneratePdfFromDocument)
                };

                var (blobAlreadyExists, pdfConversionStatus) = await context.CallActivityAsync<(bool, PdfConversionStatus)>(activityName, payload);
                if (blobAlreadyExists)
                {
                    return true;
                }

                if (pdfConversionStatus != PdfConversionStatus.DocumentConverted)
                {
                    caseEntity.SetDocumentPdfConversionFailed((payload.DocumentId.ToString(), pdfConversionStatus));
                    return false;
                }
            }
            catch (Exception exception)
            {
                caseEntity.SetDocumentPdfConversionFailed((payload.DocumentId.ToString(), PdfConversionStatus.UnexpectedError));
                log.LogMethodError(payload.CorrelationId, nameof(RefreshDocumentOrchestrator), $"Error calling {nameof(RefreshDocumentOrchestrator)}: {exception.Message}", exception);
                return false;
            }

            return true;
        }

        private async Task<(bool, PollingResult<AnalyzeResultsStats>)> EnsureOcrExists(DocumentPayload payload, IDurableOrchestrationContext context, ILogger log)
        {
            var (blobAlreadyExists, ocrPollingResult) = await GetOcrResults(context, payload);

            if (blobAlreadyExists)
            {
                return (true, null);
            }

            if (!ocrPollingResult.IsCompleted)
            {
                // Several times a day we see "stuck" OCR operations at Microsoft.  The recommendation is to a) just ditch an operation
                //  if it has not come back within an appropriate time and b) kick off another OCR operation.  So if our first OCR operation
                //  has not succeeded within the polling timeout period then let's try again.
                // Note: there is an optimisation possible here whereby we keep a handle on the first operation because it still may work eventually.
                //  We would still kick off another operation after a timeout but keep the first alive.  We'd then have a two horse race.
                //  (It is most likely that the first operation is stuck if our timeout is long enough, but it may not be stuck and just slow).
                log.LogMethodFlow(payload.CorrelationId, nameof(RefreshDocumentOrchestrator), "OCR operation did not complete within the polling timeout period, starting a fallback operation");
                (_, ocrPollingResult) = await GetOcrResults(context, payload);

                if (!ocrPollingResult.IsCompleted)
                {
                    // After second bit of the cherry, still no joy
                    throw new Exception("OCR operation did not complete within the polling timeout period");
                }
            }

            return (false, ocrPollingResult);
        }

        private async Task<(bool, PollingResult<AnalyzeResultsStats>)> GetOcrResults(IDurableOrchestrationContext context, DocumentPayload payload)
        {
            var (blobAlreadyExists, ocrOperationId) = await context.CallActivityWithRetryAsync<(bool, Guid)>(
                nameof(InitiateOcr),
                _durableActivityRetryOptions,
                payload
            );

            if (blobAlreadyExists)
            {
                return (true, null);
            }

            var pollingResult = await PollingHelper.PollActivityUntilComplete<AnalyzeResultsStats>(
                context,
                PollingHelper.CreatePollingArgs(
                    activityName: nameof(CompleteOcr),
                    activityInput: (ocrOperationId, payload),
                    prePollingDelayMs: _prePollingDelayMs,
                    pollingIntervalMs: _pollingIntervalMs,
                    maxPollingAttempts: _maxPollingAttempts,
                    activityRetryOptions: _durableActivityRetryOptions
                )
           );

            return (false, pollingResult);
        }

        private async Task<(DateTime IndexStoredTime, StoreCaseIndexesResult StoreCaseIndexesResult, PollingResult<long> PollingResult)> IndexDocument(IDurableOrchestrationContext context, DocumentPayload payload, ILogger log, IndexedDocumentEvent telemetryEvent)
        {
            var indexStoredResult = await context.CallActivityAsync<StoreCaseIndexesResult>(nameof(InitiateIndex), payload);
            var indexStoredTime = context.CurrentUtcDateTime;

            var indexPollingResult = await PollingHelper.PollActivityUntilComplete<long>(
                context,
                PollingHelper.CreatePollingArgs(
                    activityName: nameof(CompleteIndex),
                    activityInput: (payload, indexStoredResult.LineCount),
                    prePollingDelayMs: _prePollingDelayMs,
                    pollingIntervalMs: _pollingIntervalMs,
                    maxPollingAttempts: _maxPollingAttempts,
                    activityRetryOptions: _durableActivityRetryOptions
                )
            );

            if (!indexPollingResult.IsCompleted)
            {
                // Aggressive option used here: if we haven't settled then carry on rather than throwing
                // todo: in the tracker we can record a flag to say that we do not have 100% confidence in the index for this document?
                log.LogMethodFlow(payload.CorrelationId, nameof(RefreshDocumentOrchestrator), "Index operation did not complete within the polling timeout period - carrying on");
            }

            return (indexStoredTime, indexStoredResult, indexPollingResult);
        }
    }
}
