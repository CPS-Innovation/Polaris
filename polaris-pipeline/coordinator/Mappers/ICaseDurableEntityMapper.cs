using Common.Dto.Response.Documents;
using coordinator.Durable.Entity;

namespace coordinator.Functions.DurableEntity.Entity.Mapper
{
    public interface ICaseDurableEntityMapper
    {
        public TrackerDto MapCase(CaseDurableEntity caseEntity);
    }
}
