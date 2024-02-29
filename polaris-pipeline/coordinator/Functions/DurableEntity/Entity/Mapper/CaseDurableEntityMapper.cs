using Common.Dto.Tracker;
using Mapster;
using coordinator.Durable.Entity;

namespace coordinator.Functions.DurableEntity.Entity.Mapper
{
    public class CaseDurableEntityMapper
    {
        public static TrackerDto MapCase(CaseDurableEntity caseEntity)
        {
            if (caseEntity == null)
                caseEntity = new CaseDurableEntity { Status = CaseRefreshStatus.NotStarted };

            var trackerDto = caseEntity.Adapt<TrackerDto>();

            return trackerDto;
        }
    }
}
