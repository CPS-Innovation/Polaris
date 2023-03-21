using coordinator.Domain.Tracker;
using coordinator.Domain.Tracker.Presentation;

namespace coordinator.Services.DocumentToggle
{
    public interface IDocumentToggleService
    {
        PresentationFlags GetDocumentPresentationFlags(TransitionDocument document);
        bool CanReadDocument(TrackerDocument document);
        bool CanWriteDocument(TrackerDocument document);
    }
}