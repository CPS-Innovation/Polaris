using System.IO;
using System.Net.Http;
using coordinator.Clients.Ddei.Domain.CaseData.Args;

namespace coordinator.Clients.Ddei.Factories
{
    public interface IDdeiClientRequestFactory
    {
        HttpRequestMessage CreateCmsAuthValuesRequest(DdeiCmsCaseDataArgDto arg);
        HttpRequestMessage CreateUrnLookupRequest(DdeiCmsCaseIdArgDto arg);
        HttpRequestMessage CreateListCasesRequest(DdeiCmsUrnArgDto arg);
        HttpRequestMessage CreateGetCaseRequest(DdeiCmsCaseArgDto arg);
        HttpRequestMessage CreateListCaseDocumentsRequest(DdeiCmsCaseArgDto arg);
        HttpRequestMessage CreateCheckoutDocumentRequest(DdeiCmsDocumentArgDto arg);
        HttpRequestMessage CreateCancelCheckoutDocumentRequest(DdeiCmsDocumentArgDto arg);
        HttpRequestMessage CreateDocumentRequest(DdeiCmsDocumentArgDto arg);
        HttpRequestMessage CreateDocumentFromFileStoreRequest(DdeiCmsFileStoreArgDto arg);
        HttpRequestMessage CreateUploadPdfRequest(DdeiCmsDocumentArgDto arg, Stream stream);
        HttpRequestMessage CreateStatusRequest();
    }
}