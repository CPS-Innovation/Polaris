using Common.Constants;
using Common.Dto.Request;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;
using DdeiClient.Domain.Args;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DdeiClient.Factories;

public class MdsClientRequestFactory : BaseHttpClientRequestFactory, IMdsClientRequestFactory
{
    private const string UrnHeaderName = "Urn";

    public HttpRequestMessage CreateUrnLookupRequest(MdsCaseIdOnlyArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/cases/{arg.CaseId}/urn");
        AddAuthHeaders(request, arg);
        return request;
    }

    public HttpRequestMessage CreateListCasesRequest(MdsUrnArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/urns/{Encode(arg.Urn)}/case-identifiers");
        AddAuthHeaders(request, arg);
        return request;
    }

    public HttpRequestMessage CreateGetCaseRequest(MdsCaseIdentifiersArgDto arg)
    {
        throw new NotImplementedException();
    }

    public HttpRequestMessage CreateGetPcdRequestsRequest(MdsCaseIdentifiersArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/cases/{arg.CaseId}/pcd-requests/overview");
        CreateRequest(request, arg);
        return request;
    }

    public HttpRequestMessage CreateGetPcdRequest(MdsPcdArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/cases/{arg.CaseId}/pcd-request/{arg.PcdId}");
        CreateRequest(request, arg);
        return request;
    }

    public HttpRequestMessage CreateGetDefendantAndChargesRequest(MdsCaseIdentifiersArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/cases/{arg.CaseId}/defendants");
        CreateRequest(request, arg);
        return request;
    }

    public HttpRequestMessage CreateListCaseDocumentsRequest(MdsCaseIdentifiersArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/cases/{arg.CaseId}/documents/cwa");
        CreateRequest(request, arg);
        return request;
    }

    public HttpRequestMessage CreateCheckoutDocumentRequest(MdsDocumentIdAndVersionIdArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"api/cases/{arg.CaseId}/documents/{arg.DocumentId}/versions/{arg.VersionId}/checkout");
        CreateRequest(request, arg);
        return request;
    }

    public HttpRequestMessage CreateCancelCheckoutDocumentRequest(MdsDocumentIdAndVersionIdArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"api/cases/{arg.CaseId}/documents/{arg.DocumentId}/versions/{arg.VersionId}/checkout");
        CreateRequest(request, arg);
        return request;
    }

    public HttpRequestMessage CreateGetDocumentRequest(MdsDocumentIdAndVersionIdArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/cases/{arg.CaseId}/documents/{arg.DocumentId}/versions/{arg.VersionId}");
        CreateRequest(request, arg);
        return request;
    }

    public HttpRequestMessage CreateDocumentFromFileStoreRequest(MdsFileStoreArgDto arg)
    {
        throw new NotImplementedException();
    }

    public HttpRequestMessage CreateUploadPdfRequest(MdsDocumentIdAndVersionIdArgDto arg, Stream stream)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"api/cases/{arg.CaseId}/documents/{arg.DocumentId}/versions/{arg.VersionId}");
        var content = new StreamContent(stream);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        CreateRequest(request, arg, content);
        return request;
    }

    public HttpRequestMessage CreateStatusRequest()
    {
        throw new NotImplementedException();
    }

    public HttpRequestMessage CreateGetDocumentNotesRequest(MdsDocumentArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/documents/{arg.DocumentId}/notes");
        AddAuthHeaders(request, arg);
        return request;
    }

    public HttpRequestMessage CreateAddDocumentNoteRequest(MdsAddDocumentNoteArgDto arg)
    {
        var content = JsonSerializer.Serialize(new AddDocumentNoteDto
        {
            Text = arg.Text,
        });
        var request = new HttpRequestMessage(HttpMethod.Post, $"api/cases/{arg.CaseId}/documents/{arg.DocumentId}/notes");
        var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
        CreateRequest(request, arg, httpContent);
        return request;
    }

    public HttpRequestMessage CreateRenameDocumentRequest(MdsRenameDocumentArgDto arg)
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

    public HttpRequestMessage CreateRenameExhibitRequest(MdsRenameDocumentArgDto arg)
    {
        var content = JsonSerializer.Serialize(new RenameExhibitMaterialDto()
        {
            Description = arg.DocumentName,
            MaterialId = arg.DocumentId
        });
        var request = new HttpRequestMessage(HttpMethod.Patch, $"api/material/{arg.DocumentId}/rename-exhibit");
        var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
        CreateRequest(request, arg, httpContent);
        return request;
    }

    public HttpRequestMessage CreateReclassifyDocumentRequest(MdsReclassifyDocumentArgDto arg)
    {
        throw new NotImplementedException();
    }

    public HttpRequestMessage CreateReclassifyCommunicationRequest(MdsReclassifyCommunicationArgDto arg)
    {
        var content = JsonSerializer.Serialize(new ReclassifyCommunicationDto
        {
            Classification = arg.Classification,
            MaterialId = (int)arg.MaterialId,
            DocumentTypeId = arg.DocumentTypeId,
            Subject = arg.Subject,
            Statement = arg.Statement is not null
                ? new CommunicationStatementType
                {
                    Date = DateOnly.FromDateTime(DateTime.Parse(arg.Statement.Date)),
                    Witness = arg.Statement.WitnessId,
                    StatementNo = arg.Statement.StatementNo
                }
                : null,
            Exhibit = arg.Exhibit is not null
                ? new CommunicationExhibitType
                {
                    Item = arg.Exhibit.Item,
                    Reference = arg.Exhibit.Reference,
                    ExistingProducerOrWitnessId = arg.Exhibit.ExistingProducerOrWitnessId,
                    Producer = arg.Exhibit.NewProducer
                }
                : null,
            Used = arg.Used
        });
        var request = new HttpRequestMessage(HttpMethod.Post, $"api/communication/reclassify");
        AddAuthHeaders(request, arg);
        request.Content = new StringContent(content, Encoding.UTF8, ContentType.Json);
        return request;
    }

    public HttpRequestMessage CreateGetExhibitProducersRequest(MdsCaseIdentifiersArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/cases/{arg.CaseId}/producers");
        CreateRequest(request, arg);
        return request;
    }

    public HttpRequestMessage CreateCaseWitnessesRequest(MdsCaseIdentifiersArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/cases/{arg.CaseId}/witnesses");
        CreateRequest(request, arg);
        return request;
    }

    public HttpRequestMessage CreateGetMaterialTypeListRequest(CmsBaseArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/reference/reclassification");
        AddAuthHeaders(request, arg);
        return request;
    }

    public HttpRequestMessage CreateGetWitnessStatementsRequest(MdsWitnessStatementsArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/witnesses/{arg.WitnessId}/statements");
        CreateRequest(request, arg);
        return request;
    }

    public HttpRequestMessage CreateToggleIsUnusedDocumentRequest(MdsToggleIsUnusedDocumentDto dto)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"api/cases/{dto.CaseId}/documents/{dto.DocumentId}/toggle/{dto.IsUnused}");
        CreateRequest(request, dto);
        return request;
    }

    public HttpRequestMessage CreateGetCaseSummary(MdsCaseIdOnlyArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/cases/{arg.CaseId}/summary");
        CreateRequest(request, arg);
        return request;
    }

    protected override void AddAuthHeaders(HttpRequestMessage request, CmsBaseArgDto arg)
    {
        request.Headers.Add(HttpHeaderKeys.CmsAuthValues, WebUtility.UrlDecode(arg.CmsAuthValues));
        request.Headers.Add(CorrelationId, arg.CorrelationId.ToString());
        request.Headers.Add("ClientName", "CWA");
    }

    private void CreateRequest(HttpRequestMessage request, MdsUrnArgDto arg, HttpContent content = null)
    {
        AddAuthHeaders(request, arg);

        if (!string.IsNullOrEmpty(arg.Urn))
            request.Headers.Add(UrnHeaderName, arg.Urn);

        if (content is not null)
            request.Content = content;
    }
}