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
}