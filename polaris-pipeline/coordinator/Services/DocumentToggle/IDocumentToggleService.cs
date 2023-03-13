using coordinator.Domain.Tracker;

namespace coordinator.Services.DocumentToggle
{
    public interface IDocumentToggleService
    {
        void Init(string configFileContent);
        void SetDocumentPresentationStatuses(TrackerDocument document);
        bool CanReadDocument(TrackerDocument document);
        bool CanWriteDocument(TrackerDocument document);
    }
}