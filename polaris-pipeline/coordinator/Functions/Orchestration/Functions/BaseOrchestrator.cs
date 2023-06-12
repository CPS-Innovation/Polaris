using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Common.Logging;
using System;
using coordinator.Functions.DurableEntity.Entity;
using coordinator.Functions.DurableEntity.Entity.Contract;
using System.Threading.Tasks;

namespace coordinator.Functions.Orchestration.Functions
{
    public class PolarisOrchestrator
    {
        protected async Task<(ICaseDurableEntity, ICaseRefreshLogsDurableEntity)> CreateOrGetCaseDurableEntities(IDurableOrchestrationContext context, long caseId, bool newVersion, Guid correlationId, ILogger log)
        {
            log.LogMethodEntry(correlationId, nameof(CreateOrGetCaseDurableEntities), $"CaseId: {caseId}");

            var caseEntityKey = CaseDurableEntity.GetOrchestrationKey(caseId.ToString());
            var caseEntityId = new EntityId(nameof(CaseDurableEntity), caseEntityKey);
            var caseEntity = context.CreateEntityProxy<ICaseDurableEntity>(caseEntityId);

            var version = await caseEntity.GetVersion();

            if(newVersion)
            {
                version = version == null ? 1 : version + 1;
                caseEntity.SetVersion(version.Value);
            }

            var caseRefreshLogsEntityKey = CaseRefreshLogsDurableEntity.GetOrchestrationKey(caseId.ToString(), version);
            var caseRefreshLogsEntityId = new EntityId(nameof(CaseRefreshLogsDurableEntity), caseRefreshLogsEntityKey);
            var caseRefreshLogsEntity = context.CreateEntityProxy<ICaseRefreshLogsDurableEntity>(caseRefreshLogsEntityId);

            return (caseEntity, caseRefreshLogsEntity);
        }
    }
}
