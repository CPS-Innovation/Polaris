using Ddei.Domain.CaseData.Args;

namespace Ddei.Services
{
    // TODO - merge with IDdeiDocumentExtractionService
    public interface IDocumentService
    {
        Task CheckoutDocument(CmsDocumentArg arg);

        Task CancelCheckoutDocument(CmsDocumentArg arg);

        Task UploadPdf(CmsDocumentArg arg, Stream stream);
    }
}