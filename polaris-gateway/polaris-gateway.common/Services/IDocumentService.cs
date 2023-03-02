using PolarisGateway.Domain.CaseData.Args;

namespace PolarisGateway.Services
{
    public interface IDocumentService
    {
        Task CheckoutDocument(DocumentArg arg);

        Task CancelCheckoutDocument(DocumentArg arg);

        Task UploadPdf(DocumentArg arg, Stream stream, string fileName);
    }
}