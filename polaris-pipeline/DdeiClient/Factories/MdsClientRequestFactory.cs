using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;
using DdeiClient.Domain.Args;

namespace DdeiClient.Factories;

public class MdsClientRequestFactory : BaseDdeiClientRequestFactory, IDdeiClientRequestFactory
{
    private const string UrnHeaderName = "Urn";
    public HttpRequestMessage CreateVerifyCmsAuthRequest(DdeiBaseArgDto arg)
    {
        throw new NotImplementedException();
    }

    public HttpRequestMessage CreateUrnLookupRequest(DdeiCaseIdOnlyArgDto arg)
    {
        throw new NotImplementedException();
    }

    public HttpRequestMessage CreateListCasesRequest(DdeiUrnArgDto arg)
    {
        throw new NotImplementedException();
    }

    public HttpRequestMessage CreateGetCaseRequest(DdeiCaseIdentifiersArgDto arg)
    {
        throw new NotImplementedException();
    }

    public HttpRequestMessage CreateGetPcdRequestsRequest(DdeiCaseIdentifiersArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/cases/{arg.CaseId}/pcd-requests/overview");
        AddAuthHeaders(request, arg);
        request.Headers.Add(UrnHeaderName, arg.Urn);
        return request;
    }

    public HttpRequestMessage CreateGetPcdRequest(DdeiPcdArgDto arg)
    {
        throw new NotImplementedException();
    }

    public HttpRequestMessage CreateGetDefendantAndChargesRequest(DdeiCaseIdentifiersArgDto arg)
    {
        throw new NotImplementedException();
    }

    public HttpRequestMessage CreateListCaseDocumentsRequest(DdeiCaseIdentifiersArgDto arg)
    {
        throw new NotImplementedException();
    }

    public HttpRequestMessage CreateCheckoutDocumentRequest(DdeiDocumentIdAndVersionIdArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"api/cases/{arg.CaseId}/documents/{arg.DocumentId}/versions/{arg.VersionId}/checkout");
        AddAuthHeaders(request, arg);
        request.Headers.Add(UrnHeaderName, arg.Urn);
        return request;
    }

    public HttpRequestMessage CreateCancelCheckoutDocumentRequest(DdeiDocumentIdAndVersionIdArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"api/cases/{arg.CaseId}/documents/{arg.DocumentId}/versions/{arg.VersionId}/checkout");
        AddAuthHeaders(request, arg);
        request.Headers.Add(UrnHeaderName, arg.Urn);
        return request;
    }

    public HttpRequestMessage CreateGetDocumentRequest(DdeiDocumentIdAndVersionIdArgDto arg)
    {
        throw new NotImplementedException();
    }

    public HttpRequestMessage CreateDocumentFromFileStoreRequest(DdeiFileStoreArgDto arg)
    {
        throw new NotImplementedException();
    }

    public HttpRequestMessage CreateUploadPdfRequest(DdeiDocumentIdAndVersionIdArgDto arg, Stream stream)
    {
        throw new NotImplementedException();
    }

    public HttpRequestMessage CreateStatusRequest()
    {
        throw new NotImplementedException();
    }

    public HttpRequestMessage CreateGetDocumentNotesRequest(DdeiDocumentArgDto arg)
    {
        throw new NotImplementedException();
    }

    public HttpRequestMessage CreateAddDocumentNoteRequest(DdeiAddDocumentNoteArgDto arg)
    {
        throw new NotImplementedException();
    }

    public HttpRequestMessage CreateRenameDocumentRequest(DdeiRenameDocumentArgDto arg)
    {
        throw new NotImplementedException();
    }

    public HttpRequestMessage CreateReclassifyDocumentRequest(DdeiReclassifyDocumentArgDto arg)
    {
        throw new NotImplementedException();
    }

    public HttpRequestMessage CreateGetExhibitProducersRequest(DdeiCaseIdentifiersArgDto arg)
    {
        throw new NotImplementedException();
    }

    public HttpRequestMessage CreateCaseWitnessesRequest(DdeiCaseIdentifiersArgDto arg)
    {
        throw new NotImplementedException();
    }

    public HttpRequestMessage CreateGetMaterialTypeListRequest(DdeiBaseArgDto arg)
    {
        throw new NotImplementedException();
    }

    public HttpRequestMessage CreateGetWitnessStatementsRequest(DdeiWitnessStatementsArgDto arg)
    {
        throw new NotImplementedException();
    }

    public HttpRequestMessage CreateToggleIsUnusedDocumentRequest(DdeiToggleIsUnusedDocumentDto dto)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"api/cases/{dto.CaseId}/documents/{dto.DocumentId}/toggle/{dto.IsUnused}");
        AddAuthHeaders(request, dto);
        request.Headers.Add(UrnHeaderName, dto.Urn);
        return request;
    }
}