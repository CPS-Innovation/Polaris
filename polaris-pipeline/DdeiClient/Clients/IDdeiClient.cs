using Ddei.Domain;
using Ddei.Domain.CaseData.Args;

namespace Ddei.Clients
{
    public interface IDdeiClient
    {
        Task<string> GetCmsModernToken(CmsCaseDataArg arg);

        Task<IEnumerable<CaseIdentifiers>> ListCaseIdsAsync(CmsUrnArg arg);

        Task<CaseDetails> GetCaseAsync(CmsCaseArg arg);

        Task<IEnumerable<DocumentDetails>> ListCaseDocumentsAsync(CmsCaseArg arg);

        Task CheckoutDocument(CmsDocumentArg arg);

        Task CancelCheckoutDocument(CmsDocumentArg arg);

        Task UploadPdf(CmsDocumentArg arg, Stream stream);
    }
}