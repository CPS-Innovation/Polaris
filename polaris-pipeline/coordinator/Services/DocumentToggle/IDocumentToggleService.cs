using Common.Dto.FeatureFlags;
using Common.Dto.Tracker;
using coordinator.Domain.Tracker;

namespace coordinator.Services.DocumentToggle
{
    public interface IDocumentToggleService
    {
        PresentationFlagsDto GetDocumentPresentationFlags(TransitionDocument document);
        bool CanReadDocument(TrackerDocumentDto document);
        bool CanWriteDocument(TrackerDocumentDto document);
    }
}