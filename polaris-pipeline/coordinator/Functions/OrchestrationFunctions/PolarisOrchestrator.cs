using coordinator.Domain.Tracker;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Common.Logging;
using System;

namespace coordinator.Functions.OrchestrationFunctions
{
    public class PolarisOrchestrator
    {
        protected ITracker GetTracker(IDurableOrchestrationContext context, long caseId, Guid correlationId, ILogger log)
        {
            log.LogMethodEntry(correlationId, nameof(GetTracker), $"CaseId: {caseId}");

            var entityId = new EntityId(nameof(Tracker), caseId.ToString());

            log.LogMethodExit(correlationId, nameof(GetTracker), string.Empty);
            return context.CreateEntityProxy<ITracker>(entityId);
        }
    }
}
