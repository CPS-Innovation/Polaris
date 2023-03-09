using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using PolarisGateway.Domain.Logging;
using PolarisGateway.Extensions;
using System.Diagnostics;
using PolarisGateway.Domain.Exceptions;

namespace PolarisGateway.Domain.Validators
{
    [ExcludeFromCodeCoverage]
    public class AuthorizationValidator : IAuthorizationValidator
    {
        private readonly ILogger<AuthorizationValidator> _log;
        private const string ScopeType = @"http://schemas.microsoft.com/identity/claims/scope";
        private Guid _correlationId;

        public AuthorizationValidator(ILogger<AuthorizationValidator> log)
        {
            _log = log;
        }

        public async Task<bool> ValidateTokenAsync(StringValues token, Guid correlationId, string requiredScopes = null, string requiredRoles = null)
        {
            _log.LogMethodEntry(correlationId, nameof(ValidateTokenAsync), string.Empty);
            if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));
            _correlationId = correlationId;
            try
            {
                var issuer = $"https://sts.windows.net/{Environment.GetEnvironmentVariable(ConfigurationKeys.TenantId)}/";
                var audience = Environment.GetEnvironmentVariable(ConfigurationKeys.ValidAudience);
                var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(issuer + "/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever(),
                    new HttpDocumentRetriever());

                var discoveryDocument = await configurationManager.GetConfigurationAsync(default);
                var signingKeys = discoveryDocument.SigningKeys;

                var validationParameters = new TokenValidationParameters
                {
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = signingKeys,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2),
                };

                var tokenValidator = new JwtSecurityTokenHandler();
                var jwt = token.ToJwtString();
                var claimsPrincipal = tokenValidator.ValidateToken(jwt, validationParameters, out _);

                RegisterCriticalTelemetry(claimsPrincipal, correlationId);

                return IsValid(claimsPrincipal, requiredScopes, requiredRoles);
            }
            catch (InvalidOperationException invalidOperationException)
            {
                _log.LogMethodError(correlationId, nameof(ValidateTokenAsync), "An invalid operation exception was caught", invalidOperationException);
                return false;
            }
            catch (SecurityTokenValidationException securityException)
            {
                _log.LogMethodError(correlationId, nameof(ValidateTokenAsync), "A security exception was caught", securityException);
                return false;
            }
            catch (Exception ex)
            {
                _log.LogMethodError(correlationId, nameof(ValidateTokenAsync), "An unexpected error was caught", ex);
                return false;
            }
            finally
            {
                _log.LogMethodExit(correlationId, nameof(ValidateTokenAsync), string.Empty);
            }
        }

        private bool IsValid(ClaimsPrincipal claimsPrincipal, string scopes = null, string roles = null)
        {
            _log.LogMethodEntry(_correlationId, nameof(IsValid), string.Empty);

            if (claimsPrincipal == null)
            {
                _log.LogMethodFlow(_correlationId, nameof(IsValid), "Claims Principal not found - returning 'false' indicating an authorization failure");
                return false;
            }

            var requiredScopes = LoadRequiredItems(scopes);
            var requiredRoles = LoadRequiredItems(roles);

            if (!requiredScopes.Any() && !requiredRoles.Any())
            {
                _log.LogMethodFlow(_correlationId, nameof(IsValid), "No required scopes or roles found - allowing access - returning");
                return true;
            }

            var hasAccessToRoles = !requiredRoles.Any() || requiredRoles.All(claimsPrincipal.IsInRole);

            var scopeClaim = claimsPrincipal.HasClaim(x => x.Type == ScopeType)
                ? claimsPrincipal.Claims.First(x => x.Type == ScopeType).Value
                : string.Empty;

            var tokenScopes = scopeClaim.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var hasAccessToScopes = !requiredScopes.Any() || requiredScopes.All(x => tokenScopes.Any(y => string.Equals(x, y, StringComparison.OrdinalIgnoreCase)));

            _log.LogMethodExit(_correlationId, nameof(IsValid), $"Outcome role and scope checks - hasAccessToRoles: {hasAccessToRoles}, hasAccessToScopes: {hasAccessToScopes}");
            return hasAccessToRoles && hasAccessToScopes;
        }

        private void RegisterCriticalTelemetry(ClaimsPrincipal claimsPrincipal, Guid correlationId)
        {
            Activity activity = Activity.Current;
            if (activity == null)
            {
                throw new CriticalTelemetryException("System.Diagnostics.Activity.Current was expected but found to be null");
            }

            string userName = claimsPrincipal.Identity?.Name;
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new CriticalTelemetryException("User not found in ClaimsPrincipal.Identity");
            }

            try
            {
                activity.AddTag(TelemetryConstants.UserCustomDimensionName, userName);
            }
            catch (Exception exception)
            {
                throw new CriticalTelemetryException($"Unable to set {TelemetryConstants.UserCustomDimensionName}", exception);
            }

            try
            {
                activity.AddTag(TelemetryConstants.CorrelationIdCustomDimensionName, correlationId.ToString());
            }
            catch (Exception exception)
            {
                throw new CriticalTelemetryException($"Unable to set {TelemetryConstants.CorrelationIdCustomDimensionName}", exception);
            }
        }

        private static List<string> LoadRequiredItems(string items)
        {
            return string.IsNullOrWhiteSpace(items)
                ? new List<string>()
                : items.Replace(" ", string.Empty).Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}
