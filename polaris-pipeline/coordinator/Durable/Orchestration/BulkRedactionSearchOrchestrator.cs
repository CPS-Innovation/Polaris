using Common.Domain.Ocr;
using Common.Dto.Request.Redaction;
using Common.Dto.Response;
using Common.Telemetry;
using coordinator.Domain;
using coordinator.Durable.Activity;
using coordinator.Durable.Payloads;
using coordinator.TelemetryEvents;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace coordinator.Durable.Orchestration;

public class BulkRedactionSearchOrchestrator
{
    private readonly TaskOptions _durableActivityOptions = new(new TaskRetryOptions(new RetryPolicy(
        firstRetryInterval: TimeSpan.FromSeconds(5),
        maxNumberOfAttempts: 3
    )));
    private const int _prePollingDelayMs = 3000;
    private const int _pollingIntervalMs = 3000;
    private const int _maxPollingAttempts = 6;
    private readonly ITelemetryClient _telemetryClient;

    public BulkRedactionSearchOrchestrator(ITelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    [Function(nameof(BulkRedactionSearchOrchestrator))]
    public async Task<IEnumerable<RedactionDefinitionDto>> RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var payload = context.GetInput<BulkRedactionSearchPayload>();
        BulkRedactionSearchEvent telemetryEvent = default;
        try
        {
            telemetryEvent = new BulkRedactionSearchEvent(payload.CorrelationId, payload.CaseId, payload.DocumentId,
                payload.VersionId);
            //generate ocr document
            await UpdateStateAsync(context, payload, BulkRedactionSearchStatus.GeneratingOcrDocument);
            await GetOcrResults(context, payload);

            //search ocr document
            await UpdateStateAsync(context, payload, BulkRedactionSearchStatus.SearchingDocument, context.CurrentUtcDateTime);
            var bulkRedactionResult = await context.CallActivityAsync<IEnumerable<RedactionDefinitionDto>>(nameof(BulkRedactionSearchActivity), payload);

            //complete
            await UpdateStateAsync(context, payload, BulkRedactionSearchStatus.Completed, documentSearchCompletedAt: context.CurrentUtcDateTime, completedAt: context.CurrentUtcDateTime, bulkRedactionsDefinitions: bulkRedactionResult);
            return bulkRedactionResult;
        }
        catch (Exception ex)
        {
            await UpdateStateAsync(context, payload, BulkRedactionSearchStatus.Failed, failureReason: ex.Message);
            telemetryEvent.StackTrace = ex.StackTrace;
            _telemetryClient.TrackEventFailure(telemetryEvent);
            throw;
        }
    }

    private async Task GetOcrResults(TaskOrchestrationContext context, BulkRedactionSearchPayload payload)
    {
        var documentPayload = new DocumentPayload
        {
            Urn = payload.CaseUrn,
            CaseId = payload.CaseId,
            CmsAuthValues = payload.CmsAuthDetails,
            CorrelationId = payload.CorrelationId,
            DocumentId = payload.DocumentId,
            VersionId = payload.VersionId
        };
        var initiateOcrResponse = await context.CallActivityAsync<InitiateOcrResponse>(nameof(InitiateOcr), documentPayload, _durableActivityOptions);

        if (initiateOcrResponse == null && initiateOcrResponse.BlobAlreadyExists)
        {
            return;
        }

        var pollingResult = await PollingHelper.PollActivityUntilComplete<AnalyzeResultsStats>(context,
            PollingHelper.CreatePollingArgs(
                activityName: nameof(CompleteOcr),
                activityInput: new CompleteOcrPayload { OcrOperationId = initiateOcrResponse.OcrOperationId, Payload = documentPayload },
                prePollingDelayMs: _prePollingDelayMs,
                pollingIntervalMs: _pollingIntervalMs,
                maxPollingAttempts: _maxPollingAttempts,
                activityOptions: _durableActivityOptions));
    }

    private async Task UpdateStateAsync(TaskOrchestrationContext context, BulkRedactionSearchPayload payload, BulkRedactionSearchStatus status, DateTime? ocrDocumentGeneratedAt = null, DateTime? documentSearchCompletedAt = null, DateTime? completedAt = null, string failureReason = null, IEnumerable<RedactionDefinitionDto> bulkRedactionsDefinitions = null)
    {
        await context.CallActivityAsync(nameof(SetBulkRedactionSearchStatus), new BulkRedactionSearchStatusPayload
        {
            CaseId = payload.CaseId,
            DocumentId = payload.DocumentId,
            VersionId = payload.VersionId,
            SearchText = payload.SearchText,
            UpdatedAt = context.CurrentUtcDateTime,
            Status = status,
            OcrDocumentGeneratedAt = ocrDocumentGeneratedAt,
            DocumentSearchCompletedAt = documentSearchCompletedAt,
            CompletedAt = completedAt,
            FailedAt = string.IsNullOrEmpty(failureReason) ? null : context.CurrentUtcDateTime,
            FailureReason = failureReason,
            RedactionDefinitions = bulkRedactionsDefinitions
        });
    }
}