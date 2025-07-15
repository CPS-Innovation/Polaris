namespace shared.integration_tests.ApiClients;

public class CmsAuthApiClient : BaseApiClient
{
    public CmsAuthApiClient()
    {
        HttpClient = new HttpClient()
        {
            BaseAddress = new Uri("https://polaris-dev-cmsproxy.azurewebsites.net/api/")
        };
    }

    public async Task<string> GetCmsAuthTokenAsync(CancellationToken cancellationToken = default)
    {
        var form = new Dictionary<string, string>()
        {
            { "username", "mock.user" },
            { "password", "mock.user" },
        };
        var httpRequestMessage = CreateHttpRequestMessageWithForm("dev-login-full-cookie/", HttpMethod.Post, form);
        var httpResponseMessage = await SendAsync(httpRequestMessage, cancellationToken);
        return httpResponseMessage.Headers.GetValues("Set-Cookie").First();
    }
}