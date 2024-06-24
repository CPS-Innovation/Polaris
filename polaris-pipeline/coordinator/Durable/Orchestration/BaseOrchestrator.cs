using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using coordinator.Durable.Entity;

namespace coordinator.Durable.Orchestration
{
    public class BaseOrchestrator
    {
        protected ICaseDurableEntity GetEntityProxy(IDurableOrchestrationContext context, long caseId)
        {
            var caseEntityKey = InstanceIdHelper.OrchestratorKey(caseId.ToString());
            var caseEntityId = new EntityId(nameof(CaseDurableEntity), caseEntityKey);
            return context.CreateEntityProxy<ICaseDurableEntity>(caseEntityId);
        }
    }
}
