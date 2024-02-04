using Common.Dto.Tracker;
using coordinator.Durable.Entity;
using Mapster;

namespace coordinator.Mappers
{
    public class CaseDurableEntityMapper : ICaseDurableEntityMapper
    {
        public TrackerDto MapCase(CaseDurableEntity caseEntity) => caseEntity.Adapt<TrackerDto>();
    }
}
