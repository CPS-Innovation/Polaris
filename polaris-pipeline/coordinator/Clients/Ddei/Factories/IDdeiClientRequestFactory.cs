using System.IO;
using System.Net.Http;
using coordinator.Clients.Ddei.Domain.CaseData.Args;

namespace coordinator.Clients.Ddei.Factories
{
    public interface IDdeiClientRequestFactory
    {
        HttpRequestMessage CreateCmsAuthValuesRequest(DdeiBaseArg arg);
        HttpRequestMessage CreateUrnLookupRequest(DdeiGetUrnArg arg);
        HttpRequestMessage CreateListCasesRequest(DdeiUrnArg arg);
        HttpRequestMessage CreateGetCaseRequest(DdeiCaseIdArg arg);
        HttpRequestMessage CreateListCaseDocumentsRequest(DdeiCaseIdArg arg);
        HttpRequestMessage CreateCheckoutDocumentRequest(DdeiDocumentArg arg);
        HttpRequestMessage CreateCancelCheckoutDocumentRequest(DdeiDocumentArg arg);
        HttpRequestMessage CreateDocumentRequest(DdeiDocumentArg arg);
        HttpRequestMessage CreateDocumentFromFileStoreRequest(DdeiFileStoreArg arg);
        HttpRequestMessage CreateUploadPdfRequest(DdeiDocumentArg arg, Stream stream);
        HttpRequestMessage CreateStatusRequest();
    }
}