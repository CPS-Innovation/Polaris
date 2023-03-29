using Ddei.Domain.CaseData.Args;

namespace Ddei.Services
{
    // TODO - merge with IDdeiDocumentExtractionService
    public interface IDocumentService
    {
        Task CheckoutDocument(DdeiCmsDocumentArgDto arg);

        Task CancelCheckoutDocument(DdeiCmsDocumentArgDto arg);

        Task UploadPdf(DdeiCmsDocumentArgDto arg, Stream stream);
    }
}