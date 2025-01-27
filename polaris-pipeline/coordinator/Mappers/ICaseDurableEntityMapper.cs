using Common.Dto.Response.Documents;
using coordinator.Domain;

namespace coordinator.Functions.DurableEntity.Entity.Mapper
{
    public interface ICaseDurableEntityMapper
    {
        public TrackerDto MapCase(CaseDurableEntityState caseEntity, CaseDurableEntityDocumentsState documents);
    }
}
