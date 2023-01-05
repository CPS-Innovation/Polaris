using System.IO;
using System.Net.Http;
using RumpoleGateway.Domain.CaseData.Args;

namespace RumpoleGateway.CaseDataImplementations.Tde.Factories
{
    public interface ITdeClientRequestFactory
    {
        HttpRequestMessage CreateListCasesRequest(UrnArg arg);

        HttpRequestMessage CreateGetCaseRequest(CaseArg arg);

        HttpRequestMessage CreateListCaseDocumentsRequest(CaseArg arg);

        HttpRequestMessage CreateCheckoutDocumentRequest(DocumentArg arg);

        HttpRequestMessage CreateCancelCheckoutDocumentRequest(DocumentArg arg);

        HttpRequestMessage CreateUploadPdfRequest(DocumentArg arg, Stream stream, string filename);
    }
}