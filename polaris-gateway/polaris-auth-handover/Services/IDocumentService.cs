using System.IO;
using System.Threading.Tasks;
using PolarisAuthHandover.Domain.CaseData.Args;

namespace PolarisAuthHandover.Services
{
    public interface IDocumentService
    {
        Task CheckoutDocument(DocumentArg arg);

        Task CancelCheckoutDocument(DocumentArg arg);

        Task UploadPdf(DocumentArg arg, Stream stream, string fileName);
    }
}