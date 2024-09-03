
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using pdf_thumbnail_generator.Domain;
using pdf_thumbnail_generator.Durable.Orchestration;
using pdf_thumbnail_generator.Durable.Payloads;

namespace pdf_thumbnail_generator.Durable.Providers
{
  public class OrchestrationProvider : IOrchestrationProvider
  {
    private static readonly OrchestrationRuntimeStatus[] _inProgressStatuses = {
        OrchestrationRuntimeStatus.Running,
        OrchestrationRuntimeStatus.Pending,
        OrchestrationRuntimeStatus.Suspended,
    };

    private static readonly OrchestrationRuntimeStatus[] _completedStatuses = {
        OrchestrationRuntimeStatus.Completed,
        OrchestrationRuntimeStatus.Failed,
        OrchestrationRuntimeStatus.Terminated
    };

    public async Task<OrchestrationStatus> GenerateThumbnailAsync(DurableTaskClient client, ThumbnailOrchestrationPayload payload)
    {
      var instanceId = ThumbnailOrchestrator.GetKey(payload.CmsCaseId, payload.DocumentId, payload.VersionId, payload.MaxDimensionPixel);
      var existingInstance = await client.GetInstanceAsync(instanceId);

      if (existingInstance != null)
      {
        if (_inProgressStatuses.Contains(existingInstance.RuntimeStatus))
        {
          return OrchestrationStatus.InProgress;
        }

        if (_completedStatuses.Contains(existingInstance.RuntimeStatus))
        {
          return OrchestrationStatus.Completed;
        }
      }

      await client.ScheduleNewOrchestrationInstanceAsync(nameof(ThumbnailOrchestrator), payload, options: new StartOrchestrationOptions
      {
        InstanceId = instanceId
      });

      return OrchestrationStatus.Accepted;
    }
  }
}