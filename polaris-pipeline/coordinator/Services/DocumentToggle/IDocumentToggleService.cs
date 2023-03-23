using Common.Domain.Case;
using Common.Domain.Case.Presentation;
using coordinator.Domain.Tracker;

namespace coordinator.Services.DocumentToggle
{
    public interface IDocumentToggleService
    {
        PresentationFlags GetDocumentPresentationFlags(TransitionDocument document);
        bool CanReadDocument(TrackerDocument document);
        bool CanWriteDocument(TrackerDocument document);
    }
}