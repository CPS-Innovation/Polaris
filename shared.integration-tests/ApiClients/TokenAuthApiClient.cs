using NUnit.Framework;
using shared.integration_tests.Extensions;
using shared.integration_tests.Models;

namespace shared.integration_tests.ApiClients;

public class TokenAuthApiClient : BaseApiClient
{
    private readonly string _clientId;
    private readonly string _grantType;
    private readonly string _scope;
    private readonly string _clientSecret;
    private readonly string _username;
    private readonly string _password;
    public TokenAuthApiClient(TestParameters configuration)
    {
        HttpClient = new HttpClient()
        {
            BaseAddress = new Uri(configuration["TokenAuthUri"]!)
        };
        _clientId = configuration["ClientId"]!;
        _grantType = configuration["GrantType"]!;
        _scope = configuration["Scope"]!;
        _clientSecret = configuration["ClientSecret"]!;
        _username = configuration["Username"]!;
        _password = configuration["Password"]!;
    }

    public async Task<Token?> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        var form = new Dictionary<string, string>()
        {
            {"client_id", _clientId},
            {"grant_type", _grantType},
            {"scope", _scope},
            {"client_secret", _clientSecret},
            {"username", _username},
            {"password", _password},
        };
        var httpRequestMessage = CreateHttpRequestMessageWithForm(string.Empty, HttpMethod.Get, form);
        var httpResponseMessage = await SendAsync(httpRequestMessage, cancellationToken);
        var response = await httpResponseMessage.GetContentResponseAsync<Token>(cancellationToken: cancellationToken);

        return response;
    }
}