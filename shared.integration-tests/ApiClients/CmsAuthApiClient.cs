using NUnit.Framework;

namespace shared.integration_tests.ApiClients;

public class CmsAuthApiClient : BaseApiClient
{
    private readonly string _username;
    private readonly string _password;
    public CmsAuthApiClient(TestParameters configuration)
    {
        HttpClient = new HttpClient()
        {
            BaseAddress = new Uri(configuration["CmsProxyUri"]!)
        };
        _username = configuration["CmsUsername"]!;
        _password = configuration["CmsPassword"]!;
    }

    public async Task<string> GetCmsAuthTokenAsync(CancellationToken cancellationToken = default)
    {
        var form = new Dictionary<string, string>()
        {
            { "username", _username },
            { "password", _password },
        };
        var httpRequestMessage = CreateHttpRequestMessageWithForm("dev-login-full-cookie/", HttpMethod.Post, form);
        var httpResponseMessage = await SendAsync(httpRequestMessage, cancellationToken);
        return httpResponseMessage.Headers.GetValues("Set-Cookie").First();
    }
}