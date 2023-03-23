using Ddei.Domain.CaseData.Args;

namespace Ddei.Factories.Contracts
{
    public interface IDdeiClientRequestFactory
    {
        HttpRequestMessage CreateCmsModernTokenRequest(CmsCaseDataArg arg);

        HttpRequestMessage CreateListCasesRequest(CmsUrnArg arg);

        HttpRequestMessage CreateGetCaseRequest(CmsCaseArg arg);

        HttpRequestMessage CreateListCaseDocumentsRequest(CmsCaseArg arg);

        HttpRequestMessage CreateCheckoutDocumentRequest(CmsDocumentArg arg);

        HttpRequestMessage CreateCancelCheckoutDocumentRequest(CmsDocumentArg arg);

        HttpRequestMessage CreateUploadPdfRequest(CmsDocumentArg arg, Stream stream);
    }
}