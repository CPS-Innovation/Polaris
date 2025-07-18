using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;
using DdeiClient.Domain.Args;

namespace Ddei.Factories;

public interface IMdsClientRequestFactory
{
    HttpRequestMessage CreateUrnLookupRequest(DdeiCaseIdOnlyArgDto arg);

    HttpRequestMessage CreateListCasesRequest(DdeiUrnArgDto arg);

    HttpRequestMessage CreateGetCaseRequest(DdeiCaseIdentifiersArgDto arg);

    HttpRequestMessage CreateGetPcdRequestsRequest(DdeiCaseIdentifiersArgDto arg);

    HttpRequestMessage CreateGetPcdRequest(DdeiPcdArgDto arg);

    HttpRequestMessage CreateGetDefendantAndChargesRequest(DdeiCaseIdentifiersArgDto arg);

    HttpRequestMessage CreateListCaseDocumentsRequest(DdeiCaseIdentifiersArgDto arg);

    HttpRequestMessage CreateCheckoutDocumentRequest(DdeiDocumentIdAndVersionIdArgDto arg);

    HttpRequestMessage CreateCancelCheckoutDocumentRequest(DdeiDocumentIdAndVersionIdArgDto arg);

    HttpRequestMessage CreateGetDocumentRequest(DdeiDocumentIdAndVersionIdArgDto arg);

    HttpRequestMessage CreateDocumentFromFileStoreRequest(DdeiFileStoreArgDto arg);

    HttpRequestMessage CreateUploadPdfRequest(DdeiDocumentIdAndVersionIdArgDto arg, Stream stream);

    HttpRequestMessage CreateStatusRequest();

    HttpRequestMessage CreateGetDocumentNotesRequest(DdeiDocumentArgDto arg);

    HttpRequestMessage CreateAddDocumentNoteRequest(DdeiAddDocumentNoteArgDto arg);

    HttpRequestMessage CreateRenameDocumentRequest(DdeiRenameDocumentArgDto arg);

    HttpRequestMessage CreateRenameExhibitRequest(DdeiRenameDocumentArgDto arg);

    HttpRequestMessage CreateReclassifyDocumentRequest(DdeiReclassifyDocumentArgDto arg);

    HttpRequestMessage CreateReclassifyCommunicationRequest(DdeiReclassifyCommunicationArgDto arg);

    HttpRequestMessage CreateGetExhibitProducersRequest(DdeiCaseIdentifiersArgDto arg);

    HttpRequestMessage CreateCaseWitnessesRequest(DdeiCaseIdentifiersArgDto arg);

    HttpRequestMessage CreateGetMaterialTypeListRequest(DdeiBaseArgDto arg);

    HttpRequestMessage CreateGetWitnessStatementsRequest(DdeiWitnessStatementsArgDto arg);

    HttpRequestMessage CreateToggleIsUnusedDocumentRequest(DdeiToggleIsUnusedDocumentDto dto);

    HttpRequestMessage CreateGetCaseSummary(DdeiCaseIdOnlyArgDto arg);
}