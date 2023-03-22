using coordinator.Domain.Tracker;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Common.Logging;
using System;
using coordinator.Functions.DurableEntityFunctions;

namespace coordinator.Functions.OrchestrationFunctions
{
    public class PolarisOrchestrator
    {
        protected ITracker CreateOrGetTracker(IDurableOrchestrationContext context, long caseId, Guid correlationId, ILogger log)
        {
            log.LogMethodEntry(correlationId, nameof(CreateOrGetTracker), $"CaseId: {caseId}");

            var entityId = new EntityId(nameof(Tracker), caseId.ToString());

            return context.CreateEntityProxy<ITracker>(entityId);
        }
    }
}
