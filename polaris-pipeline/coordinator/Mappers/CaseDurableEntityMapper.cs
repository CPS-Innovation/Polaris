using System.Collections.Generic;
using Common.Dto.Tracker;
using coordinator.Durable.Entity;
using Mapster;

namespace coordinator.Mappers
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
