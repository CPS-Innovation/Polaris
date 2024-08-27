using Ddei.Domain.CaseData.Args;

namespace Ddei.Factories
{
    public interface IDdeiClientRequestFactory
    {
        HttpRequestMessage CreateCmsAuthValuesRequest(DdeiCmsCaseDataArgDto arg);
        HttpRequestMessage CreateUrnLookupRequest(DdeiCmsCaseIdArgDto arg);
        HttpRequestMessage CreateListCasesRequest(DdeiCmsUrnArgDto arg);
        HttpRequestMessage CreateGetCaseRequest(DdeiCmsCaseArgDto arg);
        HttpRequestMessage CreateGetPcdRequestsRequest(DdeiCmsCaseArgDto arg);
        HttpRequestMessage CreateGetPcdRequest(DdeiCmsPcdArgDto arg);
        HttpRequestMessage CreateGetDefendantAndChargesRequest(DdeiCmsCaseArgDto arg);
        HttpRequestMessage CreateListCaseDocumentsRequest(DdeiCmsCaseArgDto arg);
        HttpRequestMessage CreateCheckoutDocumentRequest(DdeiCmsDocumentArgDto arg);
        HttpRequestMessage CreateCancelCheckoutDocumentRequest(DdeiCmsDocumentArgDto arg);
        HttpRequestMessage CreateDocumentFromFileStoreRequest(DdeiCmsFileStoreArgDto arg);
        HttpRequestMessage CreateUploadPdfRequest(DdeiCmsDocumentArgDto arg, Stream stream);
        HttpRequestMessage CreateStatusRequest();
        HttpRequestMessage CreateGetDocumentNotesRequest(DdeiCmsDocumentNotesArgDto arg);
        HttpRequestMessage CreateAddDocumentNoteRequest(DdeiCmsAddDocumentNoteArgDto arg);
        HttpRequestMessage CreateRenameDocumentRequest(DdeiCmsRenameDocumentArgDto arg);
        HttpRequestMessage CreateGetExhibitProducersRequest(DdeiCmsCaseArgDto arg);
        HttpRequestMessage CreateCaseWitnessesRequest(DdeiCmsCaseArgDto arg);
        HttpRequestMessage CreateGetMaterialTypeListRequest(DdeiCmsCaseDataArgDto arg);
    }
}