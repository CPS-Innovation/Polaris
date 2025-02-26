using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Constants;
using Common.Domain.Document;
using Common.Domain.Ocr;
using Common.Dto.Response;
using Common.Logging;
using Common.Telemetry;
using coordinator.Domain;
using coordinator.Durable.Activity;
using coordinator.Durable.Payloads;
using coordinator.Durable.Payloads.Domain;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
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
        private readonly TaskOptions _durableActivityOptions = new(new TaskRetryOptions(new RetryPolicy(
            firstRetryInterval: TimeSpan.FromSeconds(5),
            maxNumberOfAttempts: 3
        )));

        public RefreshDocumentOrchestrator(ILogger<RefreshDocumentOrchestrator> log, ITelemetryClient telemetryClient)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        [Function(nameof(RefreshDocumentOrchestrator))]
        public async Task<RefreshDocumentOrchestratorResponse> Run([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var payload = context.GetInput<DocumentPayload>();
            var log = context.CreateReplaySafeLogger(nameof(RefreshDocumentOrchestrator));

            var pdfConversionResponse = await EnsurePdfExists(payload, context, log);
            if (!pdfConversionResponse.BlobAlreadyExists)
            {
                return new RefreshDocumentOrchestratorResponse { PdfConversionResponse = pdfConversionResponse, DocumentId = payload.DocumentId };
            }

            // todo: this is temporary code until the coordinator refactor exercise is done.
            if (payload.DocumentDeltaType != DocumentDeltaType.RequiresIndexing)
            {
                // return and DO NOT call SetDocumentPdfConversionSucceeded.  If we are refreshing the PDF it is because thr OCR flag has changed.
                //  The document will already either be at PdfUploadedToBlob or Indexed status.  If it is at Indexed status then we do not want to set the
                //  the flag back to PdfUploadedToBlob as Indexed is still correct.  As per comment above, all of this is to be rebuilt in pipeline refresh.
                return new RefreshDocumentOrchestratorResponse { PdfConversionResponse = pdfConversionResponse, DocumentId = payload.DocumentId };
            }

            await context.CallActivityAsync(nameof(SetDocumentPdfConversionSucceeded), new CaseIdAndDocumentIPayload { CaseId = payload.CaseId, DocumentId = payload.DocumentId });

            var telemetryEvent = new IndexedDocumentEvent(payload.CorrelationId)
            {
                CaseUrn = payload.Urn,
                CaseId = payload.CaseId,
                DocumentId = payload.DocumentId,
                DocumentTypeId = payload.DocumentType.DocumentTypeId,
                DocumentType = payload.DocumentType.DocumentType,
                DocumentCategory = payload.DocumentType.DocumentCategory,
                VersionId = payload.VersionId,
                StartTime = context.CurrentUtcDateTime,
                OperationName = nameof(RefreshDocumentOrchestrator),
            };

            // 2 START
            try
            {
                var getOcrResultsResponse = await EnsureOcrExists(payload, context, log);
                telemetryEvent.OcrCompletedTime = context.CurrentUtcDateTime;
                telemetryEvent.PageCount = getOcrResultsResponse.OcrPollingResult?.FinalResult?.PageCount ?? 0;
                telemetryEvent.LineCount = getOcrResultsResponse.OcrPollingResult?.FinalResult?.LineCount ?? 0;
                telemetryEvent.WordCount = getOcrResultsResponse.OcrPollingResult?.FinalResult?.WordCount ?? 0;

                var (indexStoredTime, indexStoredResult, indexPollingResult) = await IndexDocument(context, payload, log);

                telemetryEvent.IndexStoredTime = indexStoredTime;
                telemetryEvent.DidIndexSettle = indexPollingResult.IsCompleted;
                telemetryEvent.WaitRecordCounts = indexPollingResult.Results.Select(r => r.LineCount).ToList();
                telemetryEvent.IndexSettleTargetCount = indexStoredResult.LineCount;
                telemetryEvent.EndTime = context.CurrentUtcDateTime;

                await context.CallActivityAsync(nameof(SetDocumentIndexingSucceeded),new CaseIdAndDocumentIPayload { CaseId = payload.CaseId, DocumentId = payload.DocumentId });

                // by this point we may be replaying, so good to keep a record
                telemetryEvent.DidOrchestratorReplay = context.IsReplaying;

                // assumption here that this will not be called multiple times as a result of replaying
                //  as there should be no replaying after we've got to this point (and completed the orchestration)
                _telemetryClient.TrackEvent(telemetryEvent);

                return new RefreshDocumentOrchestratorResponse { PdfConversionResponse = pdfConversionResponse, StoreCaseIndexesResponse = indexStoredResult, DocumentId = payload.DocumentId };
            }
            catch (Exception exception)
            {
                // todo: there is no durable replay protection here, and there is evidence of several failure event records for the same failure event in our analytics.
                _telemetryClient.TrackEventFailure(telemetryEvent);

                await context.CallActivityAsync(nameof(SetDocumentIndexingFailed), new CaseIdAndDocumentIPayload { DocumentId = payload.DocumentId, CaseId = payload.CaseId });

                log.LogMethodError(payload.CorrelationId, nameof(RefreshDocumentOrchestrator), $"Error when running {nameof(RefreshDocumentOrchestrator)} orchestration: {exception.Message}", exception);

                return new RefreshDocumentOrchestratorResponse { PdfConversionResponse = pdfConversionResponse, StoreCaseIndexesResponse = new StoreCaseIndexesResult { IsSuccess = false }, DocumentId = payload.DocumentId };
            }
        }

        private async Task<PdfConversionResponse> EnsurePdfExists(DocumentPayload payload, TaskOrchestrationContext context, ILogger log)
        {
            try
            {
                var activityName = payload.DocumentNatureType switch
                {
                    DocumentNature.Types.PreChargeDecisionRequest => nameof(GeneratePdfFromPcdRequest),
                    DocumentNature.Types.DefendantsAndCharges => nameof(GeneratePdfFromDefendantsAndCharges),
                    _ => nameof(GeneratePdfFromDocument)
                };

                var pdfConversionResponse = await context.CallActivityAsync<PdfConversionResponse>(activityName, payload);
                if (pdfConversionResponse.BlobAlreadyExists)
                {
                    return pdfConversionResponse;
                }

                if (pdfConversionResponse.PdfConversionStatus != PdfConversionStatus.DocumentConverted)
                {
                    await context.CallActivityAsync(nameof(SetDocumentPdfConversionFailed), new SetDocumentPdfConversionStatusPayload { CaseId = payload.CaseId, DocumentId = payload.DocumentId, PdfConversionStatus = pdfConversionResponse.PdfConversionStatus });

                    return pdfConversionResponse;
                }
            }
            catch (Exception exception)
            {
                await context.CallActivityAsync(nameof(SetDocumentPdfConversionFailed), new SetDocumentPdfConversionStatusPayload { CaseId = payload.CaseId, DocumentId = payload.DocumentId, PdfConversionStatus = PdfConversionStatus.UnexpectedError });

                log.LogMethodError(payload.CorrelationId, nameof(RefreshDocumentOrchestrator), $"Error calling {nameof(RefreshDocumentOrchestrator)}: {exception.Message}", exception);
                return new PdfConversionResponse { BlobAlreadyExists = false, PdfConversionStatus = PdfConversionStatus.UnexpectedError };
            }

            return new PdfConversionResponse { BlobAlreadyExists = true, PdfConversionStatus = PdfConversionStatus.DocumentConverted };
        }

        private async Task<OcrOperationPollingResult> EnsureOcrExists(DocumentPayload payload, TaskOrchestrationContext context, ILogger log)
        {
            var getOcrResultsResponse = await GetOcrResults(context, payload);

            if (getOcrResultsResponse.BlobAlreadyExists)
            {
                return new OcrOperationPollingResult { BlobAlreadyExists = true };
            }

            if (!getOcrResultsResponse.OcrPollingResult.IsCompleted)
            {
                // Several times a day we see "stuck" OCR operations at Microsoft.  The recommendation is to a) just ditch an operation
                //  if it has not come back within an appropriate time and b) kick off another OCR operation.  So if our first OCR operation
                //  has not succeeded within the polling timeout period then let's try again.
                // Note: there is an optimisation possible here whereby we keep a handle on the first operation because it still may work eventually.
                //  We would still kick off another operation after a timeout but keep the first alive.  We'd then have a two horse race.
                //  (It is most likely that the first operation is stuck if our timeout is long enough, but it may not be stuck and just slow).
                log.LogMethodFlow(payload.CorrelationId, nameof(RefreshDocumentOrchestrator), "OCR operation did not complete within the polling timeout period, starting a fallback operation");
                getOcrResultsResponse = await GetOcrResults(context, payload);

                if (getOcrResultsResponse.OcrPollingResult?.IsCompleted == false)
                {
                    // After second bit of the cherry, still no joy
                    throw new Exception("OCR operation did not complete within the polling timeout period");
                }
            }

            return new OcrOperationPollingResult { BlobAlreadyExists = false, OcrPollingResult = getOcrResultsResponse.OcrPollingResult };
        }

        private async Task<OcrOperationPollingResult> GetOcrResults(TaskOrchestrationContext context, DocumentPayload payload)
        {
            var initiateOcrResponse = await context.CallActivityAsync<InitiateOcrResponse>(
                nameof(InitiateOcr),
                payload,
                _durableActivityOptions);

            if (initiateOcrResponse.BlobAlreadyExists)
            {
                return new OcrOperationPollingResult { BlobAlreadyExists = true };
            }

            var pollingResult = await PollingHelper.PollActivityUntilComplete<AnalyzeResultsStats>(
                context,
                PollingHelper.CreatePollingArgs(
                    activityName: nameof(CompleteOcr),
                    activityInput: new CompleteOcrPayload { OcrOperationId = initiateOcrResponse.OcrOperationId, Payload = payload },
                    prePollingDelayMs: _prePollingDelayMs,
                    pollingIntervalMs: _pollingIntervalMs,
                    maxPollingAttempts: _maxPollingAttempts,
                    activityOptions: _durableActivityOptions));

            return new OcrOperationPollingResult { BlobAlreadyExists = false, OcrPollingResult = pollingResult };
        }

        private async Task<(DateTime IndexStoredTime, StoreCaseIndexesResult StoreCaseIndexesResult, PollingResult<CompleteIndexResponse> PollingResult)> IndexDocument(TaskOrchestrationContext context, DocumentPayload payload, ILogger log)
        {
            var indexStoredResult = await context.CallActivityAsync<StoreCaseIndexesResult>(nameof(InitiateIndex), payload);
            var indexStoredTime = context.CurrentUtcDateTime;

            var indexPollingResult = await PollingHelper.PollActivityUntilComplete<CompleteIndexResponse>(
                context,
                PollingHelper.CreatePollingArgs(
                    activityName: nameof(CompleteIndex),
                    activityInput: new CompleteIndexPayload { Payload = payload, TargetCount = indexStoredResult.LineCount },
                    prePollingDelayMs: _prePollingDelayMs,
                    pollingIntervalMs: _pollingIntervalMs,
                    maxPollingAttempts: _maxPollingAttempts,
                    activityOptions: _durableActivityOptions
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