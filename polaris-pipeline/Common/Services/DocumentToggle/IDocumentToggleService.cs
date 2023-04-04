using Common.Dto.Document;
using Common.Dto.FeatureFlags;
using Common.Dto.Tracker;

namespace Common.Services.DocumentToggle
{
    public interface IDocumentToggleService
    {
        PresentationFlagsDto GetDocumentPresentationFlags(TransitionDocumentDto document);
        bool CanReadDocument(TrackerDocumentDto document);
        bool CanWriteDocument(TrackerDocumentDto document);
    }
}