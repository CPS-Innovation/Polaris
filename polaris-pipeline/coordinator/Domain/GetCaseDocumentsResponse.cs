using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Case;
using Common.Dto.Response.Document;

namespace coordinator.Domain;

public class GetCaseDocumentsResponse
{
    public GetCaseDocumentsResponse(CmsDocumentDto[] cmsDocuments, PcdRequestCoreDto[] pcdRequests, DefendantsAndChargesListDto defendantAndCharges)
    {
        CmsDocuments = cmsDocuments;
        PcdRequests = pcdRequests;
        DefendantAndCharges = defendantAndCharges;
    }

    public CmsDocumentDto[] CmsDocuments { get; set; }

    public PcdRequestCoreDto[] PcdRequests { get; set; }

    public DefendantsAndChargesListDto DefendantAndCharges { get; set; }
}
