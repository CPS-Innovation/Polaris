using Common.Dto.Tracker;
using Mapster;
using coordinator.Durable.Entity;

namespace coordinator.Functions.DurableEntity.Entity.Mapper
{
    public class CaseDurableEntityMapper : ICaseDurableEntityMapper
    {
        public TrackerDto MapCase(CaseDurableEntity caseEntity)
        {
            if (caseEntity == null)
            {
                caseEntity = new CaseDurableEntity { Status = CaseRefreshStatus.NotStarted };
            }

            return caseEntity.Adapt<TrackerDto>();
        }
    }
}
