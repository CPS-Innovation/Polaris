using Common.Domain.Entity;
using Common.Dto.Case;
using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using Common.Dto.FeatureFlags;

namespace coordinator.Services.DocumentToggle
{
    public interface IDocumentToggleService
    {
        PresentationFlagsDto GetDocumentPresentationFlags(CmsDocumentDto document);
        PresentationFlagsDto GetPcdRequestPresentationFlags(PcdRequestDto pcdRequest);
        PresentationFlagsDto GetDefendantAndChargesPresentationFlags(DefendantsAndChargesListDto defendantAndCharges);

        bool CanReadDocument(BaseDocumentEntity document);
        bool CanWriteDocument(BaseDocumentEntity document);
    }
}