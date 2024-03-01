using Common.Dto.Tracker;
using Mapster;
using coordinator.Durable.Entity;
using System.Collections.Generic;

namespace coordinator.Functions.DurableEntity.Entity.Mapper
{
    public class CaseDurableEntityMapper : ICaseDurableEntityMapper
    {
        public TrackerDto MapCase(CaseDurableEntity caseEntity) =>
            caseEntity == null
                // todo: not certain that we need to handle a null caseEntity
                ? new TrackerDto
                {
                    Documents = new List<DocumentDto>(),
                }
                : caseEntity.Adapt<TrackerDto>();
    }
}
