using Common.Dto.Case;
using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using Common.Dto.FeatureFlags;
using Common.Dto.Tracker;

namespace Common.Services.DocumentToggle
{
    public interface IDocumentToggleService
    {
        PresentationFlagsDto GetDocumentPresentationFlags(DocumentDto document);
        PresentationFlagsDto GetPcdRequestPresentationFlags(PcdRequestDto pcdRequest);
        PresentationFlagsDto GetDefendantAndChargesPresentationFlags(DefendantAndChargesDto defendantAndCharges);

        bool CanReadDocument(TrackerDocumentDto document);
        bool CanWriteDocument(TrackerDocumentDto document);
    }
}