using coordinator.Domain.Tracker;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Common.Logging;
using System;
using coordinator.Functions.DurableEntity.Entity;

namespace coordinator.Functions.Orchestration.Functions
{
    public class PolarisOrchestrator
    {
        protected ITrackerEntity CreateOrGetTracker(IDurableOrchestrationContext context, long caseId, Guid correlationId, ILogger log)
        {
            log.LogMethodEntry(correlationId, nameof(CreateOrGetTracker), $"CaseId: {caseId}");

            var entityId = new EntityId(nameof(TrackerEntity), caseId.ToString());

            return context.CreateEntityProxy<ITrackerEntity>(entityId);
        }
    }
}
