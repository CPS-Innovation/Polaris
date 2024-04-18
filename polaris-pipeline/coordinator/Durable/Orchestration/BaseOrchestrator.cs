using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using coordinator.Durable.Entity;
using System.Threading.Tasks;

namespace coordinator.Durable.Orchestration
{
    public class BaseOrchestrator
    {
        protected async Task<ICaseDurableEntity> CreateOrGetCaseDurableEntity(IDurableOrchestrationContext context, long caseId, bool newVersion, Guid correlationId, ILogger log)
        {
            var caseEntityKey = RefreshCaseOrchestrator.GetKey(caseId.ToString());
            var caseEntityId = new EntityId(nameof(CaseDurableEntity), caseEntityKey);
            var caseEntity = context.CreateEntityProxy<ICaseDurableEntity>(caseEntityId);

            var version = await caseEntity.GetVersion();

            if (newVersion)
            {
                version = version == null ? 1 : version + 1;
                caseEntity.SetVersion(version.Value);
            }

            return caseEntity;
        }
    }
}
