// <copyright file="CleardownService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Services.ClearDownService;

using Common.Configuration;
using Common.Services.BlobStorage;
using Common.Telemetry;
using coordinator.Clients.TextExtractor;
using coordinator.Durable.Providers;
using coordinator.Functions.Maintenance;
using coordinator.TelemetryEvents;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

public class ClearDownService(Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
  ITextExtractorClient textExtractorClient,
  IOrchestrationProvider orchestrationProvider,
  ILogger<ClearDownService> logger,
  IConfiguration configuration)
    : IClearDownService
{
    private readonly IPolarisBlobStorageService polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty) ?? throw new ArgumentNullException(nameof(blobStorageServiceFactory));

    public async Task DeleteCaseAsync(DurableTaskClient client, string caseUrn, int caseId, Guid correlationId, bool isLegacy = true, bool removeBlobs = true)
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
            logger.LogInformation("Calling text extractor remove case indexes {CaseId}", caseId);

            var deleteResult = await textExtractorClient.RemoveCaseIndexesAsync(caseUrn, caseId, correlationId, isLegacy);

            logger.LogInformation("Text extractor remove case indexes Completed {CaseId}", caseId);
            telemetryEvent.RemovedCaseIndexTime = DateTime.UtcNow;
            telemetryEvent.AttemptedRemovedDocumentCount = deleteResult.DocumentCount;
            telemetryEvent.SuccessfulRemovedDocumentCount = deleteResult.SuccessCount;
            telemetryEvent.FailedRemovedDocumentCount = deleteResult.FailureCount;

            if (removeBlobs)
            {
                logger.LogInformation("Deleting blobs with prefix: {CaseId}", caseId);
                await this.polarisBlobStorageService.DeleteBlobsByPrefixAsync(caseId);
                logger.LogInformation("Deleted blobs with prefix: {CaseId}", caseId);
                telemetryEvent.BlobsDeletedTime = DateTime.UtcNow;
            }

            logger.LogInformation("Deleting case orchestration: {CaseId}", caseId);
            var orchestrationResult = await orchestrationProvider.DeleteCaseOrchestrationAsync(client, caseId);
            telemetryEvent.TerminatedInstancesCount = orchestrationResult.TerminatedInstancesCount;
            telemetryEvent.GotTerminateInstancesTime = orchestrationResult.GotTerminateInstancesDateTime;
            telemetryEvent.DidOrchestrationsTerminate = orchestrationResult.DidOrchestrationsTerminate;
            telemetryEvent.TerminatedInstancesSettledTime = orchestrationResult.TerminatedInstancesSettledDateTime;
            telemetryEvent.GotPurgeInstancesTime = orchestrationResult.GotPurgeInstancesDateTime;
            telemetryEvent.PurgeInstancesCount = orchestrationResult.PurgeInstancesCount;
            telemetryEvent.PurgedInstancesCount = orchestrationResult.PurgedInstancesCount;
            logger.LogInformation("Deleted case orchestration: {CaseId}", caseId);

            if (orchestrationResult.IsSuccess)
            {
                telemetryEvent.EndTime = orchestrationResult.OrchestrationEndDateTime;
                logger.TrackEvent(telemetryEvent);
            }
            else
            {
                throw new Exception($"DeleteCaseOrchestrationAsync failed");
            }
        }
        catch (Exception ex)
        {
            logger.TrackEventFailure(telemetryEvent);
            throw new InvalidOperationException($"Error deleting case {caseId}", ex);
        }
    }
}
