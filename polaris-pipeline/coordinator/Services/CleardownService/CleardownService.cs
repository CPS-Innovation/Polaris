using Common.Services.BlobStorage;
using Common.Telemetry;
using coordinator.Clients.TextExtractor;
using coordinator.Durable.Providers;
using coordinator.TelemetryEvents;
using System;
using System.Threading.Tasks;
using Common.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.DurableTask.Client;
using coordinator.Functions.Maintenance;

namespace coordinator.Services.ClearDownService
{
    public class ClearDownService : IClearDownService
    {
        private readonly IPolarisBlobStorageService _polarisBlobStorageService;
        private readonly ITextExtractorClient _textExtractorClient;
        private readonly IOrchestrationProvider _orchestrationProvider;
        private readonly ITelemetryClient _telemetryClient;

        public ClearDownService(Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
          ITextExtractorClient textExtractorClient,
          IOrchestrationProvider orchestrationProvider,
          ITelemetryClient telemetryClient,
          IConfiguration configuration)
        {
            _polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty) ?? throw new ArgumentNullException(nameof(blobStorageServiceFactory));
            _textExtractorClient = textExtractorClient;
            _orchestrationProvider = orchestrationProvider;
            _telemetryClient = telemetryClient;
        }

        public async Task DeleteCaseAsync(DurableTaskClient client, string caseUrn, int caseId, Guid correlationId)
        {
            var telemetryEvent = new DeletedCaseEvent(
                correlationId,
                caseId,
                DateTime.UtcNow)
            {
                OperationName = nameof(DeleteCase),
            };
            try
            {
                _telemetryClient.TrackTrace($"Calling text extractor remove case indexes {caseId}");
                var deleteResult = await _textExtractorClient.RemoveCaseIndexesAsync(caseUrn, caseId, correlationId);
                _telemetryClient.TrackTrace($"Text extractor remove case indexes Completed {caseId}");
                telemetryEvent.RemovedCaseIndexTime = DateTime.UtcNow;
                telemetryEvent.AttemptedRemovedDocumentCount = deleteResult.DocumentCount;
                telemetryEvent.SuccessfulRemovedDocumentCount = deleteResult.SuccessCount;
                telemetryEvent.FailedRemovedDocumentCount = deleteResult.FailureCount;

                _telemetryClient.TrackTrace($"Deleting blobs with prefix: {caseId}");
                await _polarisBlobStorageService.DeleteBlobsByPrefixAsync(caseId);
                _telemetryClient.TrackTrace($"Deleted blobs with prefix: {caseId}");
                telemetryEvent.BlobsDeletedTime = DateTime.UtcNow;

                _telemetryClient.TrackTrace($"Deleting case orchestration: {caseId}");
                var orchestrationResult = await _orchestrationProvider.DeleteCaseOrchestrationAsync(client, caseId);
                telemetryEvent.TerminatedInstancesCount = orchestrationResult.TerminatedInstancesCount;
                telemetryEvent.GotTerminateInstancesTime = orchestrationResult.GotTerminateInstancesDateTime;
                telemetryEvent.DidOrchestrationsTerminate = orchestrationResult.DidOrchestrationsTerminate;
                telemetryEvent.TerminatedInstancesSettledTime = orchestrationResult.TerminatedInstancesSettledDateTime;
                telemetryEvent.GotPurgeInstancesTime = orchestrationResult.GotPurgeInstancesDateTime;
                telemetryEvent.PurgeInstancesCount = orchestrationResult.PurgeInstancesCount;
                telemetryEvent.PurgedInstancesCount = orchestrationResult.PurgedInstancesCount;
                _telemetryClient.TrackTrace($"Deleted case orchestration: {caseId}");

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
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex);
                _telemetryClient.TrackEventFailure(telemetryEvent);
                throw;
            }
        }
    }
}