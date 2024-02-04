using Common.Dto.Tracker;
using coordinator.Durable.Entity;

namespace coordinator.Mappers
{
    public interface ICaseDurableEntityMapper
    {
        public TrackerDto MapCase(CaseDurableEntity caseEntity);
    }
}
