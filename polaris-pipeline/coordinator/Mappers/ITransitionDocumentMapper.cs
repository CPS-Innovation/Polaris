using Common.Domain.DocumentExtraction;
using coordinator.Domain.Tracker;

namespace coordinator.Mappers
{
    public interface ITransitionDocumentMapper
    {
        TransitionDocument MapToTransitionDocument(CmsCaseDocument cmsCaseDocument);
    }
}