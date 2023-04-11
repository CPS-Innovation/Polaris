using Ddei.Domain.CaseData.Args;

namespace Ddei.Factories.Contracts
{
    public interface IDdeiClientRequestFactory
    {
        HttpRequestMessage CreateCmsModernTokenRequest(DdeiCmsCaseDataArgDto arg);

        HttpRequestMessage CreateListCasesRequest(DdeiCmsUrnArgDto arg);

        HttpRequestMessage CreateGetCaseRequest(DdeiCmsCaseArgDto arg);

        HttpRequestMessage CreateListCaseDocumentsRequest(DdeiCmsCaseArgDto arg);

        HttpRequestMessage CreateCheckoutDocumentRequest(DdeiCmsDocumentArgDto arg);

        HttpRequestMessage CreateCancelCheckoutDocumentRequest(DdeiCmsDocumentArgDto arg);

        HttpRequestMessage CreateDocumentRequest(DdeiCmsDocumentArgDto arg);

        HttpRequestMessage CreateUploadPdfRequest(DdeiCmsDocumentArgDto arg, Stream stream);

    }
}