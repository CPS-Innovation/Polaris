using shared.integration_tests.Extensions;
using shared.integration_tests.Models;

namespace shared.integration_tests.ApiClients;

public class TokenAuthApiClient : BaseApiClient
{
    //private string clientId = "3649c1c8-00cf-4b8f-a671-304bc074937c";
    //private string grantType = "password";
    //private string scope = "https://CPSGOVUK.onmicrosoft.com/fa-polaris-dev-gateway/user_impersonation";
    //private string clientSecret = "UNm8Q~-d~Fquwoyf1vB8FKtYFHq-Vlwlfc2gVahz";
    //private string username = "AutomationUser.ServiceTeam2@cps.gov.uk";
    //private string password = "Loxu8024";
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