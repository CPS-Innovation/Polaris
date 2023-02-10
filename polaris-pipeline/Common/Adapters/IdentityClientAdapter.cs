using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Constants;
using Common.Domain.Exceptions;
using Common.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace Common.Adapters
{
    [ExcludeFromCodeCoverage]
    public class IdentityClientAdapter : IIdentityClientAdapter
    {
        private readonly IConfiguration _configuration;
        private readonly IConfidentialClientApplication _confidentialClientApplication;
        private readonly ILogger<IdentityClientAdapter> _logger;
        
        public IdentityClientAdapter(IConfiguration configuration, IConfidentialClientApplication confidentialClientApplication, ILogger<IdentityClientAdapter> logger)
        {
            _configuration = configuration;
            _confidentialClientApplication = confidentialClientApplication ??
                                             throw new ArgumentNullException(nameof(confidentialClientApplication));
            _logger = logger;
        }

        public async Task<string> GetAccessTokenOnBehalfOfAsync(string currentAccessToken, string scopes, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GetAccessTokenOnBehalfOfAsync), scopes);

            try
            {
                var userAssertion = new UserAssertion(currentAccessToken, AuthenticationKeys.AzureAuthenticationAssertionType);
                var requestedScopes = new Collection<string> {scopes};

#if DEBUG
                if (_configuration.IsSettingEnabled(DebugSettings.MockOnBehalfOfTokenClient))
                {
                    return $"{nameof(IdentityClientAdapter)}.{nameof(GetAccessTokenOnBehalfOfAsync)} Mock Token";
                }
                else
                {
                    var result = await _confidentialClientApplication
                                        .AcquireTokenOnBehalfOf(requestedScopes, userAssertion)
                                        .WithCorrelationId(correlationId).ExecuteAsync();
                    return result.AccessToken;
                }
#else
                var result = await _confidentialClientApplication
                                    .AcquireTokenOnBehalfOf(requestedScopes, userAssertion)
                                    .WithCorrelationId(correlationId).ExecuteAsync();
                return result.AccessToken;
#endif

            }
            catch (MsalException exception)
            {
                throw new OnBehalfOfTokenClientException(
                    $"Failed to acquire onBehalfOf token. Exception: {exception.Message}");
            }
            finally
            {
                _logger.LogMethodExit(correlationId, nameof(GetAccessTokenOnBehalfOfAsync), string.Empty);
            }
        }

        public async Task<string> GetClientAccessTokenAsync(string scopes, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GetClientAccessTokenAsync), scopes);

            try
            {
                var requestedScopes = new Collection<string> {scopes};
                
                /*var isLocal = string.IsNullOrEmpty(Environment.GetEnvironmentVariable(ConfigKeys.SharedKeys.WebsiteInstanceId));
                if (isLocal)
                {
                    _logger.LogMethodFlow(correlationId, nameof(GetClientAccessTokenAsync), "In debug mode... bypassing authentication checks...");
                    return "[Token Placeholder]";
                }*/
                
                var result = await _confidentialClientApplication.AcquireTokenForClient(requestedScopes)
                    .WithCorrelationId(correlationId).ExecuteAsync();
                return result.AccessToken;
            }
            catch (MsalUiRequiredException ex)
            {
                throw new ClientTokenException(
                    $"Failed to acquire a client token. Insufficient permissions. Exception: {ex.Message}");
            }
            catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
            {
                throw new ClientTokenException(
                    $"Failed to acquire a client token. Invalid scope. Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new ClientTokenException($"Failed to acquire a client token. Exception: {ex.Message}");
            }
            finally
            {
                _logger.LogMethodExit(correlationId, nameof(GetClientAccessTokenAsync), string.Empty);
            }
        }
    }
}
