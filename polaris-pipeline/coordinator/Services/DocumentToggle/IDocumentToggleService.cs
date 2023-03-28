using Common.Domain.Case.Polaris;
using Common.Domain.Case.Presentation;
using Common.Domain.Case.Tracker;
using coordinator.Domain.Tracker;

namespace coordinator.Services.DocumentToggle
{
    public interface IDocumentToggleService
    {
        PresentationFlags GetDocumentPresentationFlags(TransitionDocument document);
        bool CanReadDocument(TrackerDocumentDto document);
        bool CanWriteDocument(TrackerDocumentDto document);
    }
}