using Common.Domain.Case.Document;
using coordinator.Domain.Tracker;

namespace coordinator.Mappers
{
    public interface ITransitionDocumentMapper
    {
        TransitionDocument MapToTransitionDocument(DocumentDto cmsCaseDocument);
    }
}