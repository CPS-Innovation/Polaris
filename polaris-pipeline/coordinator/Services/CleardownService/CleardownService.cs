using Common.Services.BlobStorage;
using Common.Telemetry;
using coordinator.Clients.TextExtractor;
using coordinator.Durable.Providers;
using coordinator.TelemetryEvents;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Threading.Tasks;

namespace coordinator.Services.CleardownService
{
  public class CleardownService : ICleardownService
  {
    private readonly IPolarisBlobStorageService _polarisBlobStorageService;
    private readonly ITextExtractorClient _textExtractorClient;
    private readonly IOrchestrationProvider _orchestrationProvider;
    private readonly ITelemetryClient _telemetryClient;

    public CleardownService(IPolarisBlobStorageService polarisStorageService,
      ITextExtractorClient textExtractorClient,
      IOrchestrationProvider orchestrationProvider,
      ITelemetryClient telemetryClient)
    {
      _polarisBlobStorageService = polarisStorageService;
      _textExtractorClient = textExtractorClient;
      _orchestrationProvider = orchestrationProvider;
      _telemetryClient = telemetryClient;
    }

    public async Task DeleteCaseAsync(IDurableOrchestrationClient client, string caseUrn, int caseId, Guid correlationId)
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


        await _polarisBlobStorageService.DeleteBlobsByPrefixAsync(caseId);
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