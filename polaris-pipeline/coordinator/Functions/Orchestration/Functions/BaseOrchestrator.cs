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
        protected async Task<(ICaseEntity, ICaseRefreshLogsEntity)> CreateOrGetCaseTrackersForEntity(IDurableOrchestrationContext context, long caseId, Guid correlationId, ILogger log)
        {
            log.LogMethodEntry(correlationId, nameof(CreateOrGetCaseTrackersForEntity), $"CaseId: {caseId}");

            var caseEntityId = new EntityId(nameof(CaseEntity), $"{caseId}");
            var caseEntity = context.CreateEntityProxy<ICaseEntity>(caseEntityId);

            var version = await caseEntity.GetVersion();
            var caseRefreshLogsEntityId = new EntityId(nameof(CaseRefreshLogsEntity), $"{caseId}-{version}");
            var caseRefreshLogsEntity = context.CreateEntityProxy<ICaseRefreshLogsEntity>(caseRefreshLogsEntityId);

            return (caseEntity, caseRefreshLogsEntity);
        }
        //protected ICaseRefreshLogsEntity CreateOrGetCaseRefreshLogsEntity(IDurableOrchestrationContext context, long caseId, int version, Guid correlationId, ILogger log)
        //{
        //    log.LogMethodEntry(correlationId, nameof(CreateOrGetCaseRefreshLogsEntity), $"CaseId: {caseId}, version: {version}");

        //    var entityId = new EntityId(nameof(CaseRefreshLogsEntity), $"{caseId}-{version}");
        //    var entity = context.CreateEntityProxy<ICaseRefreshLogsEntity>(entityId);

        //    return entity;
        //}
    }
}
