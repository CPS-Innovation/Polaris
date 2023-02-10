using System;
using Microsoft.Identity.Client;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolarisGateway.Domain.Logging;
using Microsoft.Extensions.Configuration;

namespace PolarisGateway.Clients.OnBehalfOfTokenClient
{
    [ExcludeFromCodeCoverage]
    public class OnBehalfOfTokenClient : IOnBehalfOfTokenClient
    {
        private readonly IConfidentialClientApplication _application;
        private readonly ILogger<OnBehalfOfTokenClient> _logger;
        private readonly IConfiguration _configuration;

        public OnBehalfOfTokenClient(IConfiguration configuration, IConfidentialClientApplication application, ILogger<OnBehalfOfTokenClient> logger)
        {
            _configuration = configuration;
            _application = application;
            _logger = logger;
        }

        public async Task<string> GetAccessTokenAsync(string accessToken, string scope, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GetAccessTokenAsync), scope);

#if DEBUG
            if (_configuration.IsSettingEnabled("POLARIS_MOCK_ON_BEHALF_OF_TOKEN_CLIENT"))
            {
                return $"{nameof(OnBehalfOfTokenClient)}.{nameof(GetAccessTokenAsync)} Mock Token";
            }
#endif
            var scopes = new Collection<string> { scope };
            var userAssertion = new UserAssertion(accessToken, AuthenticationKeys.AzureAuthenticationAssertionType);
            var result = await _application.AcquireTokenOnBehalfOf(scopes, userAssertion).ExecuteAsync();

            var generatedToken = result.AccessToken;
            _logger.LogMethodExit(correlationId, nameof(GetAccessTokenAsync), generatedToken);
            return generatedToken;
        }
    }
}
