using Common.Dto.Tracker;
using coordinator.Durable.Entity;

namespace coordinator.Functions.DurableEntity.Entity.Mapper
{
    public interface ICaseDurableEntityMapper
    {
        public TrackerDto MapCase(CaseDurableEntity caseEntity);
    }
}
