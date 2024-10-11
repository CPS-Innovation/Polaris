using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;

namespace Ddei.Factories
{
    public interface IDdeiClientRequestFactory
    {
        HttpRequestMessage CreateVerifyCmsAuthRequest(DdeiBaseArgDto arg);
        HttpRequestMessage CreateUrnLookupRequest(DdeiCaseIdOnlyArgDto arg);
        HttpRequestMessage CreateListCasesRequest(DdeiUrnArgDto arg);
        HttpRequestMessage CreateGetCaseRequest(DdeiCaseIdentifiersArgDto arg);
        HttpRequestMessage CreateGetPcdRequestsRequest(DdeiCaseIdentifiersArgDto arg);
        HttpRequestMessage CreateGetPcdRequest(DdeiCmsPcdArgDto arg);
        HttpRequestMessage CreateGetDefendantAndChargesRequest(DdeiCaseIdentifiersArgDto arg);
        HttpRequestMessage CreateListCaseDocumentsRequest(DdeiCaseIdentifiersArgDto arg);
        HttpRequestMessage CreateCheckoutDocumentRequest(DdeiCmsDocumentIdAndVersionIdArgDto arg);
        HttpRequestMessage CreateCancelCheckoutDocumentRequest(DdeiCmsDocumentIdAndVersionIdArgDto arg);
        HttpRequestMessage CreateDocumentFromFileStoreRequest(DdeiCmsFileStoreArgDto arg);
        HttpRequestMessage CreateUploadPdfRequest(DdeiCmsDocumentIdAndVersionIdArgDto arg, Stream stream);
        HttpRequestMessage CreateStatusRequest();
        HttpRequestMessage CreateGetDocumentNotesRequest(DdeiCmsDocumentNotesArgDto arg);
        HttpRequestMessage CreateAddDocumentNoteRequest(DdeiCmsAddDocumentNoteArgDto arg);
        HttpRequestMessage CreateRenameDocumentRequest(DdeiCmsRenameDocumentArgDto arg);
        HttpRequestMessage CreateReclassifyDocumentRequest(DdeiCmsReclassifyDocumentArgDto arg);
        HttpRequestMessage CreateGetExhibitProducersRequest(DdeiCaseIdentifiersArgDto arg);
        HttpRequestMessage CreateCaseWitnessesRequest(DdeiCaseIdentifiersArgDto arg);
        HttpRequestMessage CreateGetMaterialTypeListRequest(DdeiBaseArgDto arg);
        HttpRequestMessage CreateGetWitnessStatementsRequest(DdeiCmsWitnessStatementsArgDto arg);
    }
}