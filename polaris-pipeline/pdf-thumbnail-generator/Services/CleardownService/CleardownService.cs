
using System.Text.RegularExpressions;
using Common.Services.BlobStorageService;
using Common.Telemetry;
using Microsoft.DurableTask.Client;
using pdf_thumbnail_generator.Durable.Providers;

namespace pdf_thumbnail_generator.Services.CleardownService;

public class CleardownService : ICleardownService
{
  private readonly IPolarisBlobStorageService _blobStorageService;
  private readonly IOrchestrationProvider _orchestrationProvider;
  private readonly ITelemetryClient _telemetryClient;

  public CleardownService(IPolarisBlobStorageService blobStorageService,
  IOrchestrationProvider orchestrationProvider,
  ITelemetryClient telemetryClient)
  {
    _blobStorageService = blobStorageService;
    _orchestrationProvider = orchestrationProvider;
    _telemetryClient = telemetryClient;
  }

  public async Task DeleteCaseThumbnailAsync(DurableTaskClient client, string caseUrn, string instanceId, DateTime earliestDateToKeep, Guid correlationId)
  {
    try
    {
      var caseId = ExtractCaseIdFromInstanceId(instanceId);
      await _blobStorageService.DeleteBlobsByCaseAsync(caseId);
      await _orchestrationProvider.DeleteCaseThumbnailOrchestrationAsync(client, instanceId, earliestDateToKeep);
    }
    catch (Exception ex)
    {
      // log error
    }
  }

  private string ExtractCaseIdFromInstanceId(string instanceId)
  {
    var regex = new Regex(@"^\[(\d+)\]");
    var match = regex.Match(instanceId);

    if (match.Success)
    {
      return match.Groups[1].Value;
    }
    throw new FormatException("Invalid instanceId format. Cannot extract caseId.");
  }

}