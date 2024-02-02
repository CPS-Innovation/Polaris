using Common.Dto.Tracker;
using coordinator.Functions.DurableEntity.Entity;
using Mapster;

namespace coordinator.Mappers
{
    public class CaseDurableEntityMapper : ICaseDurableEntityMapper
    {
        public TrackerDto MapCase(CaseDurableEntity caseEntity)
        {
            if (caseEntity == null)
                caseEntity = new CaseDurableEntity { Status = CaseRefreshStatus.NotStarted };

            var trackerDto = caseEntity.Adapt<TrackerDto>();

            return trackerDto;
        }
    }
}
