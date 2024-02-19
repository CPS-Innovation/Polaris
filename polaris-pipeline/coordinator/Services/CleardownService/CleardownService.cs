using Common.Services.BlobStorageService.Contracts;
using Common.Telemetry.Contracts;
using coordinator.Clients.Contracts;
using coordinator.Providers;
using coordinator.Services.TextExtractService;
using coordinator.TelemetryEvents;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Threading.Tasks;

namespace coordinator.Services.CleardownService
{
  public class CleardownService : ICleardownService
  {
    private readonly IPolarisBlobStorageService _blobStorageService;
    private readonly ITextExtractorClient _textExtractorClient;
    private readonly ITextExtractService _textExtractorService;
    private readonly IOrchestrationProvider _orchestrationProvider;
    private readonly ITelemetryClient _telemetryClient;

    public CleardownService(IPolarisBlobStorageService blobStorageService,
      ITextExtractorClient textExtractorClient,
      ITextExtractService textExtractorService,
      IOrchestrationProvider orchestrationProvider,
      ITelemetryClient telemetryClient)
    {
      _blobStorageService = blobStorageService;
      _textExtractorClient = textExtractorClient;
      _textExtractorService = textExtractorService;
      _orchestrationProvider = orchestrationProvider;
      _telemetryClient = telemetryClient;
    }

    public async Task DeleteCaseAsync(IDurableOrchestrationClient client, string caseUrn, int caseId, Guid correlationId, bool waitForIndexToSettle)
    {
      var telemetryEvent = new DeletedCaseEvent(
          correlationId: correlationId,
          caseId: caseId,
          startTime: DateTime.UtcNow
      );
      try
      {
        var deleteResult = await _textExtractorClient.RemoveCaseIndexesAsync(caseUrn, caseId, correlationId);
        telemetryEvent.RemovedCaseIndexTime = DateTime.UtcNow;
        telemetryEvent.AttemptedRemovedDocumentCount = deleteResult.DocumentCount;
        telemetryEvent.SuccessfulRemovedDocumentCount = deleteResult.SuccessCount;
        telemetryEvent.FailedRemovedDocumentCount = deleteResult.FailureCount;

        if (waitForIndexToSettle)
        {
          var waitResult = await _textExtractorService.WaitForCaseEmptyResultsAsync(caseUrn, caseId, correlationId);
          telemetryEvent.DidIndexSettle = waitResult.IsSuccess;
          telemetryEvent.WaitRecordCounts = waitResult.RecordCounts;
          telemetryEvent.IndexSettledTime = DateTime.UtcNow;
        }
        telemetryEvent.DidWaitForIndexToSettle = waitForIndexToSettle;


        await _blobStorageService.DeleteBlobsByCaseAsync(caseId.ToString());
        telemetryEvent.BlobsDeletedTime = DateTime.UtcNow;

        var orchestrationResult = await _orchestrationProvider.DeleteCaseOrchestrationAsync(client, caseId);
        telemetryEvent.TerminatedInstancesCount = orchestrationResult.TerminatedInstancesCount;
        telemetryEvent.GotTerminateInstancesTime = orchestrationResult.GotTerminateInstancesDateTime;
        telemetryEvent.DidOrchestrationsTerminate = orchestrationResult.DidOrchestrationsTerminate;
        telemetryEvent.TerminatedInstancesSettledTime = orchestrationResult.TerminatedInstancesSettledDateTime;
        telemetryEvent.GotPurgeInstancesTime = orchestrationResult.GotPurgeInstancesDateTime;
        telemetryEvent.PurgeInstancesCount = orchestrationResult.PurgeInstancesCount;
        telemetryEvent.PurgedInstancesCount = orchestrationResult.PurgedInstancesCount;

        if (orchestrationResult.IsSuccess)
        {
          telemetryEvent.EndTime = orchestrationResult.OrchestrationEndDateTime;
          _telemetryClient.TrackEvent(telemetryEvent);
        }
        else
        {
          throw new Exception($"DeleteCaseOrchestrationAsync failed");
        }
      }
      catch (Exception)
      {
        _telemetryClient.TrackEventFailure(telemetryEvent);
        throw;
      }
    }
  }
}