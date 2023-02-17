using System.IO;
using System.Net.Http;
using PolarisAuthHandover.Domain.CaseData.Args;

namespace PolarisAuthHandover.CaseDataImplementations.Ddei.Factories
{
    public interface IDdeiClientRequestFactory
    {
        HttpRequestMessage CreateCmsModernTokenRequest(CaseDataArg arg);

        HttpRequestMessage CreateListCasesRequest(UrnArg arg);

        HttpRequestMessage CreateGetCaseRequest(CaseArg arg);

        HttpRequestMessage CreateListCaseDocumentsRequest(CaseArg arg);

        HttpRequestMessage CreateCheckoutDocumentRequest(DocumentArg arg);

        HttpRequestMessage CreateCancelCheckoutDocumentRequest(DocumentArg arg);

        HttpRequestMessage CreateUploadPdfRequest(DocumentArg arg, Stream stream, string filename);
    }
}