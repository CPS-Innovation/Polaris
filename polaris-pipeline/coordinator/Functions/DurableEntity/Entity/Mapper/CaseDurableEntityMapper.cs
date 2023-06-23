using Common.Dto.Tracker;
using Mapster;

namespace coordinator.Functions.DurableEntity.Entity.Mapper
{
    public class CaseDurableEntityMapper
    {
        public static TrackerDto MapCase(CaseDurableEntity caseEntity, CaseRefreshLogsDurableEntity caseRefreshLogsEntity)
        {
            if(caseEntity == null)
                caseEntity = new CaseDurableEntity { Status = CaseRefreshStatus.NotStarted };

            if(caseRefreshLogsEntity == null)
                caseRefreshLogsEntity = new CaseRefreshLogsDurableEntity();

            var trackerDto = (caseEntity, caseRefreshLogsEntity).Adapt<TrackerDto>();

            return trackerDto;
        }
    }
}
