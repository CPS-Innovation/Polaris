
using System.Text.RegularExpressions;
using Common.Services.BlobStorage;
using Common.Telemetry;
using Microsoft.DurableTask.Client;
using pdf_thumbnail_generator.Durable.Providers;
using pdf_thumbnail_generator.TelemetryEvents;

namespace pdf_thumbnail_generator.Services.ClearDownService;

public class ClearDownService : IClearDownService
{ 
    private readonly IPolarisBlobStorageService _blobStorageServiceContainerThumbnails;
    private readonly IOrchestrationProvider _orchestrationProvider;
    private readonly ITelemetryClient _telemetryClient;
    
    public ClearDownService(Func<string, IPolarisBlobStorageService> blobStorageServiceFactory, IOrchestrationProvider orchestrationProvider, ITelemetryClient telemetryClient) 
    { 
        _blobStorageServiceContainerThumbnails = blobStorageServiceFactory("Thumbnails");
        _orchestrationProvider = orchestrationProvider;
        _telemetryClient = telemetryClient;
    }

    public async Task DeleteCaseThumbnailAsync(DurableTaskClient client, string caseUrn, string instanceId, DateTime earliestDateToKeep, Guid correlationId)
    { 
        var telemetryEvent = new DeleteCaseThumbnailEvent(correlationId: correlationId, instanceId: instanceId, startTime: DateTime.UtcNow);
        
        try
        { 
            var caseId = ExtractCaseIdFromInstanceId(instanceId);
            await _blobStorageServiceContainerThumbnails.DeleteBlobsByPrefixAsync(int.Parse(caseId));
            telemetryEvent.BlobsDeletedTime = DateTime.UtcNow;

            var orchestrationResult = await _orchestrationProvider.DeleteCaseThumbnailOrchestrationAsync(client, instanceId, earliestDateToKeep);
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
                throw new Exception($"DeleteCaseThumbnailOrchestrationAsync failed");
            }
        }
        catch (Exception)
        { 
            _telemetryClient.TrackEventFailure(telemetryEvent);
            throw;
        }
    }
    
    private static string ExtractCaseIdFromInstanceId(string instanceId)
    { 
        var regex = new Regex(@"^\[(\d+)\]", RegexOptions.None, TimeSpan.FromMilliseconds(100));
        var match = regex.Match(instanceId);

        if (match.Success)
            return match.Groups[1].Value;
    
        throw new FormatException("Invalid instanceId format. Cannot extract caseId.");
    }
}