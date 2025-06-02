using Common.Dto.Request;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;
using DdeiClient.Domain.Args;
using System.Text;
using System.Text.Json;

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
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/cases/{arg.CaseId}/urn");
        AddAuthHeaders(request, arg);
        return request;
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
        CreateRequest(request, arg);
        return request;
    }

    public HttpRequestMessage CreateGetPcdRequest(DdeiPcdArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/cases/{arg.CaseId}/pcd-request/{arg.PcdId}");
        CreateRequest(request, arg);
        return request;
    }

    public HttpRequestMessage CreateGetDefendantAndChargesRequest(DdeiCaseIdentifiersArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/cases/{arg.CaseId}/defendants");
        CreateRequest(request, arg);
        return request;
    }

    public HttpRequestMessage CreateListCaseDocumentsRequest(DdeiCaseIdentifiersArgDto arg)
    {
        throw new NotImplementedException();
    }

    public HttpRequestMessage CreateCheckoutDocumentRequest(DdeiDocumentIdAndVersionIdArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"api/cases/{arg.CaseId}/documents/{arg.DocumentId}/versions/{arg.VersionId}/checkout");
        CreateRequest(request, arg);
        return request;
    }

    public HttpRequestMessage CreateCancelCheckoutDocumentRequest(DdeiDocumentIdAndVersionIdArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"api/cases/{arg.CaseId}/documents/{arg.DocumentId}/versions/{arg.VersionId}/checkout");
        CreateRequest(request, arg);
        return request;
    }

    public HttpRequestMessage CreateGetDocumentRequest(DdeiDocumentIdAndVersionIdArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/cases/{arg.CaseId}/documents/{arg.DocumentId}/versions/{arg.VersionId}");
        CreateRequest(request, arg);
        return request;
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
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/documents/{arg.DocumentId}/notes");
        AddAuthHeaders(request, arg);
        return request;
    }

    public HttpRequestMessage CreateAddDocumentNoteRequest(DdeiAddDocumentNoteArgDto arg)
    {
        var content = JsonSerializer.Serialize(new AddDocumentNoteDto
        {
            Text = arg.Text
        });
        var request = new HttpRequestMessage(HttpMethod.Post, $"api/cases/{arg.CaseId}/documents/{arg.DocumentId}/notes");
        var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
        CreateRequest(request, arg, httpContent);
        return request;
    }

    public HttpRequestMessage CreateRenameDocumentRequest(DdeiRenameDocumentArgDto arg)
    {
        var content = JsonSerializer.Serialize(new RenameMaterialDto()
        {
            Subject = arg.DocumentName,
            MaterialId = arg.DocumentId
        });
        var request = new HttpRequestMessage(HttpMethod.Patch, $"api/material/{arg.DocumentId}/rename");
        var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
        CreateRequest(request, arg, httpContent);
        return request;
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
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/witnesses/{arg.WitnessId}/statements");
        CreateRequest(request, arg);
        return request;
    }

    public HttpRequestMessage CreateToggleIsUnusedDocumentRequest(DdeiToggleIsUnusedDocumentDto dto)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"api/cases/{dto.CaseId}/documents/{dto.DocumentId}/toggle/{dto.IsUnused}");
        CreateRequest(request, dto);
        return request;
    }

    public HttpRequestMessage CreateGetCaseSummary(DdeiCaseIdOnlyArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/cases/{arg.CaseId}/summary");
        AddAuthHeaders(request, arg);
        return request;
    }

    private void CreateRequest(HttpRequestMessage request, DdeiUrnArgDto arg, HttpContent content = null)
    {
        AddAuthHeaders(request, arg);
        request.Headers.Add(UrnHeaderName, arg.Urn);

        if (content is not null)
            request.Content = content;
    }
}