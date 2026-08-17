using Common.Services.BlobStorage;
using Common.Telemetry;
using coordinator.Clients.TextExtractor;
using coordinator.Durable.Providers;
using coordinator.TelemetryEvents;
using System;
using System.Threading.Tasks;
using Common.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.DurableTask.Client;
using coordinator.Functions.Maintenance;

namespace coordinator.Services.ClearDownService
{
    public class ClearDownService : IClearDownService
    {
        private readonly IPolarisBlobStorageService _polarisBlobStorageService;
        private readonly ITextExtractorClient _textExtractorClient;
        private readonly IOrchestrationProvider _orchestrationProvider;
        private readonly ILogger<ClearDownService> _logger;

        public ClearDownService(Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
          ITextExtractorClient textExtractorClient,
          IOrchestrationProvider orchestrationProvider,
          ILogger<ClearDownService> logger,
          IConfiguration configuration)
        {
            _polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty) ?? throw new ArgumentNullException(nameof(blobStorageServiceFactory));
            _textExtractorClient = textExtractorClient;
            _orchestrationProvider = orchestrationProvider;
            _logger = logger;
        }

        public async Task DeleteCaseAsync(DurableTaskClient client, string caseUrn, int caseId, Guid correlationId)
        {
            var telemetryEvent = new DeletedCaseEvent(
                correlationId,
                caseId,
                DateTime.UtcNow)
            {
                OperationName = nameof(DeleteCaseLegacy),
            };
            try
            {
                _logger.LogInformation("Calling text extractor remove case indexes {CaseId}", caseId);
                var deleteResult = await _textExtractorClient.RemoveCaseIndexesAsync(caseUrn, caseId, correlationId);
                _logger.LogInformation("Text extractor remove case indexes Completed {CaseId}", caseId);
                telemetryEvent.RemovedCaseIndexTime = DateTime.UtcNow;
                telemetryEvent.AttemptedRemovedDocumentCount = deleteResult.DocumentCount;
                telemetryEvent.SuccessfulRemovedDocumentCount = deleteResult.SuccessCount;
                telemetryEvent.FailedRemovedDocumentCount = deleteResult.FailureCount;

                _logger.LogInformation("Deleting blobs with prefix: {CaseId}", caseId);
                await _polarisBlobStorageService.DeleteBlobsByPrefixAsync(caseId);
                _logger.LogInformation("Deleted blobs with prefix: {CaseId}", caseId);
                telemetryEvent.BlobsDeletedTime = DateTime.UtcNow;

                _logger.LogInformation("Deleting case orchestration: {CaseId}", caseId);
                var orchestrationResult = await _orchestrationProvider.DeleteCaseOrchestrationAsync(client, caseId);
                telemetryEvent.TerminatedInstancesCount = orchestrationResult.TerminatedInstancesCount;
                telemetryEvent.GotTerminateInstancesTime = orchestrationResult.GotTerminateInstancesDateTime;
                telemetryEvent.DidOrchestrationsTerminate = orchestrationResult.DidOrchestrationsTerminate;
                telemetryEvent.TerminatedInstancesSettledTime = orchestrationResult.TerminatedInstancesSettledDateTime;
                telemetryEvent.GotPurgeInstancesTime = orchestrationResult.GotPurgeInstancesDateTime;
                telemetryEvent.PurgeInstancesCount = orchestrationResult.PurgeInstancesCount;
                telemetryEvent.PurgedInstancesCount = orchestrationResult.PurgedInstancesCount;
                _logger.LogInformation("Deleted case orchestration: {CaseId}", caseId);

                if (orchestrationResult.IsSuccess)
                {
                    telemetryEvent.EndTime = orchestrationResult.OrchestrationEndDateTime;
                    _logger.TrackEvent(telemetryEvent);
                }
                else
                {
                    throw new Exception($"DeleteCaseOrchestrationAsync failed");
                }
            }
            catch (Exception ex)
            {
                _logger.TrackEventFailure(telemetryEvent);
                throw new InvalidOperationException($"Error deleting case {caseId}", ex);
            }
        }
    }
}