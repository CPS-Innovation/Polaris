using Common.Constants;
using Common.Dto.Request;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;
using System.Text;
using System.Text.Json;

namespace DdeiClient.Factories;

public class DdeiAuthClientRequestFactory : BaseHttpClientRequestFactory, IDdeiAuthClientRequestFactory
{
    public HttpRequestMessage CreateVerifyCmsAuthRequest(CmsBaseArgDto arg)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/verify-cms-auth");
        AddAuthHeaders(request, arg);
        return request;
    }

}