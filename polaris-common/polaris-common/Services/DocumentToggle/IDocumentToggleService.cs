using polaris_common.Domain.Entity;
using polaris_common.Dto.Case;
using polaris_common.Dto.Case.PreCharge;
using polaris_common.Dto.Document;
using polaris_common.Dto.FeatureFlags;

namespace polaris_common.Services.DocumentToggle
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