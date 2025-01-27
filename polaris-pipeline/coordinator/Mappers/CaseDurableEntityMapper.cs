using Common.Dto.Response.Documents;
using Mapster;
using coordinator.Domain;

namespace coordinator.Functions.DurableEntity.Entity.Mapper
{
    public class CaseDurableEntityMapper : ICaseDurableEntityMapper
    {
        public TrackerDto MapCase(CaseDurableEntityState caseEntity, CaseDurableEntityDocumentsState documents) =>
            caseEntity == null
                // todo: not certain that we need to handle a null caseEntity
                ? new TrackerDto
                {
                    Documents = [],
                }
                : (caseEntity, documents).Adapt<TrackerDto>();
    }
}
