using Ddei.Domain;
using Ddei.Domain.CaseData.Args;

namespace Ddei.Clients
{
    public interface IDdeiClient
    {
        Task<string> GetCmsModernToken(DdeiCmsCaseDataArgDto arg);

        Task<IEnumerable<DdeiCaseIdentifiersDto>> ListCaseIdsAsync(DdeiCmsUrnArgDto arg);

        Task<DdeiCaseDetailsDto> GetCaseAsync(DdeiCmsCaseArgDto arg);

        Task<IEnumerable<DdeiDocumentDetailsDto>> ListCaseDocumentsAsync(DdeiCmsCaseArgDto arg);

        Task CheckoutDocument(DdeiCmsDocumentArgDto arg);

        Task CancelCheckoutDocument(DdeiCmsDocumentArgDto arg);

        Task UploadPdf(DdeiCmsDocumentArgDto arg, Stream stream);
    }
}