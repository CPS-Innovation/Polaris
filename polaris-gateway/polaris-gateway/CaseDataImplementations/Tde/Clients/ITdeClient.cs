using System.Collections.Generic;
using System.Threading.Tasks;
using PolarisGateway.Domain.CaseData.Args;
using PolarisGateway.CaseDataImplementations.Ddei.Domain;
using System.IO;

namespace PolarisGateway.CaseDataImplementations.Ddei.Clients
{
    public interface IDdeiClient
    {
        Task<IEnumerable<CaseIdentifiers>> ListCaseIdsAsync(UrnArg arg);

        Task<CaseDetails> GetCaseAsync(CaseArg arg);

        Task<IEnumerable<DocumentDetails>> ListCaseDocumentsAsync(CaseArg arg);

        Task CheckoutDocument(DocumentArg arg);

        Task CancelCheckoutDocument(DocumentArg arg);

        Task UploadPdf(DocumentArg arg, Stream stream, string filename);
    }
}