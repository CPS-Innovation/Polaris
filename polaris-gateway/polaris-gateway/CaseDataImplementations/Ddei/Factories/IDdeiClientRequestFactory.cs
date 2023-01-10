using System.IO;
using System.Net.Http;
using PolarisGateway.Domain.CaseData.Args;

namespace PolarisGateway.CaseDataImplementations.Ddei.Factories
{
    public interface IDdeiClientRequestFactory
    {
        HttpRequestMessage CreateListCasesRequest(UrnArg arg);

        HttpRequestMessage CreateGetCaseRequest(CaseArg arg);

        HttpRequestMessage CreateListCaseDocumentsRequest(CaseArg arg);

        HttpRequestMessage CreateCheckoutDocumentRequest(DocumentArg arg);

        HttpRequestMessage CreateCancelCheckoutDocumentRequest(DocumentArg arg);

        HttpRequestMessage CreateUploadPdfRequest(DocumentArg arg, Stream stream, string filename);
    }
}