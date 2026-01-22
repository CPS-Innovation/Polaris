using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;

namespace DdeiClient.Factories;

public class DdeiAuthClientRequestFactory : BaseDdeiClientRequestFactory, IDdeiAuthClientRequestFactory
{
    public HttpRequestMessage CreateVerifyCmsAuthRequest(DdeiBaseArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/verify-cms-auth");
        AddAuthHeaders(request, arg);
        return request;
    }

    public HttpRequestMessage CreateGetDocumentRequest(DdeiDocumentIdAndVersionIdArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/urns/{arg.Urn}/cases/{arg.CaseId}/documents/{arg.DocumentId}/versions/{arg.VersionId}");
        AddAuthHeaders(request, arg);
        return request;
    }
}