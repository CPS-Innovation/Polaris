using shared.integration_tests.Extensions;
using shared.integration_tests.Models;

namespace shared.integration_tests.ApiClients;

public class TokenAuthApiClient : BaseApiClient
{
    
    public TokenAuthApiClient()
    {
        HttpClient = new HttpClient()
        {
            BaseAddress = new Uri("https://login.microsoftonline.com/00dd0d1d-d7e6-4338-ac51-565339c7088c/oauth2/v2.0/token")
        };
    }

    public async Task<Token?> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        var form = new Dictionary<string, string>()
        {
            //{"client_id", clientId},
            //{"grant_type", grantType},
            //{"scope", scope},
            //{"client_secret", clientSecret},
            //{"username", username},
            //{"password", password},
        };
        var httpRequestMessage = CreateHttpRequestMessageWithForm(string.Empty, HttpMethod.Get, form);
        var httpResponseMessage = await SendAsync(httpRequestMessage, cancellationToken);
        var response = await httpResponseMessage.GetContentResponseAsync<Token>(cancellationToken: cancellationToken);

        return response;
    }
}