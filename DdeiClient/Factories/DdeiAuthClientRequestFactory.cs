using Common.Constants;
using Common.Dto.Request;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;
using System.Text;
using System.Text.Json;

namespace DdeiClient.Factories;

public class DdeiAuthClientRequestFactory : BaseDdeiClientRequestFactory, IDdeiAuthClientRequestFactory
{
    public HttpRequestMessage CreateVerifyCmsAuthRequest(MdsBaseArgDto arg) // VERIFY if MdsBaseArgDto is correct here
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/verify-cms-auth");
        AddAuthHeaders(request, arg);
        return request;
    }

    public HttpRequestMessage CreateReclassifyDocumentRequest(MdsReclassifyDocumentArgDto arg)
    {
        var content = JsonSerializer.Serialize(new ReclassifyDocumentDto
        {
            DocumentTypeId = arg.DocumentTypeId,
            Exhibit = arg.Exhibit,
            Statement = arg.Statement,
            Other = arg.Other,
            Immediate = arg.Immediate
        });
        var request = new HttpRequestMessage(HttpMethod.Put, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents/{arg.DocumentId}/reclassify");
        AddAuthHeaders(request, arg);
        request.Content = new StringContent(content, Encoding.UTF8, ContentType.Json);
        return request;
    }

    public HttpRequestMessage CreateRenameDocumentRequest(MdsRenameDocumentArgDto arg)
    {
        var content = JsonSerializer.Serialize(new RenameDocumentDto
        {
            DocumentName = arg.DocumentName
        });
        var request = new HttpRequestMessage(HttpMethod.Put, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents/{arg.DocumentId}/rename");
        AddAuthHeaders(request, arg);
        request.Content = new StringContent(content, Encoding.UTF8, "application/json");
        return request;
    }
}