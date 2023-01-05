using System.IO;
using System.Threading.Tasks;
using RumpoleGateway.Domain.CaseData.Args;

namespace RumpoleGateway.Services
{
    public interface IDocumentService
    {
        Task CheckoutDocument(DocumentArg arg);

        Task CancelCheckoutDocument(DocumentArg arg);

        Task UploadPdf(DocumentArg arg, Stream stream, string fileName);
    }
}