using System.Collections.Generic;
using System.Threading.Tasks;
using RumpoleGateway.Domain.CaseData.Args;
using RumpoleGateway.CaseDataImplementations.Tde.Domain;
using System.IO;

namespace RumpoleGateway.CaseDataImplementations.Tde.Clients
{
    public interface ITdeClient
    {
        Task<IEnumerable<CaseIdentifiers>> ListCaseIdsAsync(UrnArg arg);

        Task<CaseDetails> GetCaseAsync(CaseArg arg);

        Task<IEnumerable<DocumentDetails>> ListCaseDocumentsAsync(CaseArg arg);

        Task CheckoutDocument(DocumentArg arg);

        Task CancelCheckoutDocument(DocumentArg arg);

        Task UploadPdf(DocumentArg arg, Stream stream, string filename);
    }
}