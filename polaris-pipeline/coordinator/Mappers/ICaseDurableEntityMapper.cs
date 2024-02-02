using Common.Dto.Tracker;
using coordinator.Functions.DurableEntity.Entity;

namespace coordinator.Mappers
{
    public interface ICaseDurableEntityMapper
    {
        public TrackerDto MapCase(CaseDurableEntity caseEntity);
    }
}
