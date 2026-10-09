using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Common.Dto.Response.Document.FeatureFlags;
using Common.Dto.Response.HouseKeeping.Pcd;

namespace Common.Services.DocumentToggle
{
    public interface IDocumentToggleService
    {
        PresentationFlagsDto GetDocumentPresentationFlags(CmsDocumentDto document);
        PresentationFlagsDto GetPcdRequestPresentationFlags(Dto.Response.HouseKeeping.Pcd.PcdRequestDto pcdRequest);
        PresentationFlagsDto GetDefendantAndChargesPresentationFlags(DefendantsAndChargesListDto defendantAndCharges);
    }
}