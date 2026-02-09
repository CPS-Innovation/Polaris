using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;
using DdeiClient.Domain.Args;

namespace Ddei.Factories;

public interface IMdsClientRequestFactory
{
    HttpRequestMessage CreateUrnLookupRequest(MdsCaseIdOnlyArgDto arg);

    HttpRequestMessage CreateListCasesRequest(MdsUrnArgDto arg);

    HttpRequestMessage CreateGetCaseRequest(MdsCaseIdentifiersArgDto arg);

    HttpRequestMessage CreateGetPcdRequestsRequest(MdsCaseIdentifiersArgDto arg);

    HttpRequestMessage CreateGetPcdRequest(MdsPcdArgDto arg);

    HttpRequestMessage CreateGetDefendantAndChargesRequest(MdsCaseIdentifiersArgDto arg);

    HttpRequestMessage CreateListCaseDocumentsRequest(MdsCaseIdentifiersArgDto arg);

    HttpRequestMessage CreateCheckoutDocumentRequest(MdsDocumentIdAndVersionIdArgDto arg);

    HttpRequestMessage CreateCancelCheckoutDocumentRequest(MdsDocumentIdAndVersionIdArgDto arg);

    HttpRequestMessage CreateGetDocumentRequest(MdsDocumentIdAndVersionIdArgDto arg);

    HttpRequestMessage CreateDocumentFromFileStoreRequest(MdsFileStoreArgDto arg);

    HttpRequestMessage CreateUploadPdfRequest(MdsDocumentIdAndVersionIdArgDto arg, Stream stream);

    HttpRequestMessage CreateStatusRequest();

    HttpRequestMessage CreateGetDocumentNotesRequest(MdsDocumentArgDto arg);

    HttpRequestMessage CreateAddDocumentNoteRequest(MdsAddDocumentNoteArgDto arg);

    HttpRequestMessage CreateRenameDocumentRequest(MdsRenameDocumentArgDto arg);

    HttpRequestMessage CreateRenameExhibitRequest(MdsRenameDocumentArgDto arg);

    HttpRequestMessage CreateReclassifyDocumentRequest(MdsReclassifyDocumentArgDto arg);

    HttpRequestMessage CreateReclassifyCommunicationRequest(MdsReclassifyCommunicationArgDto arg);

    HttpRequestMessage CreateGetExhibitProducersRequest(MdsCaseIdentifiersArgDto arg);

    HttpRequestMessage CreateCaseWitnessesRequest(MdsCaseIdentifiersArgDto arg);

    HttpRequestMessage CreateGetMaterialTypeListRequest(CmsBaseArgDto arg);

    HttpRequestMessage CreateGetWitnessStatementsRequest(MdsWitnessStatementsArgDto arg);

    HttpRequestMessage CreateToggleIsUnusedDocumentRequest(MdsToggleIsUnusedDocumentDto dto);

    HttpRequestMessage CreateGetCaseSummary(MdsCaseIdOnlyArgDto arg);
}
