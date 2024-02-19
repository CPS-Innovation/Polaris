using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Common.Constants;
using Common.Extensions;
using Common.Logging;
using PolarisGateway.Domain.Validation;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace PolarisGateway.Domain.Validators
{
    [ExcludeFromCodeCoverage]
    public class AuthorizationValidator : IAuthorizationValidator
    {
        private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
        private readonly ILogger<AuthorizationValidator> _log;
        private const string ScopeType = @"http://schemas.microsoft.com/identity/claims/scope";

        public AuthorizationValidator(ConfigurationManager<OpenIdConnectConfiguration> configurationManager, ILogger<AuthorizationValidator> log)
        {
            _configurationManager = configurationManager;
            _log = log;
        }

        public async Task<ValidateTokenResult> ValidateTokenAsync(StringValues token, Guid correlationId, string requiredScopes = null, string requiredRoles = null)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));
            try
            {
                var audience = Environment.GetEnvironmentVariable(OAuthSettings.ValidAudience);
                var discoveryDocument = await _configurationManager.GetConfigurationAsync(default);

                var validationParameters = new TokenValidationParameters
                {
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ValidateIssuer = true,
                    ValidIssuer = discoveryDocument.Issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = discoveryDocument.SigningKeys,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2),
                };

                var tokenValidator = new JwtSecurityTokenHandler();
                var claimsPrincipal = tokenValidator.ValidateToken(token.ToJwtString(), validationParameters, out _);

                var isValid = IsValid(claimsPrincipal, requiredScopes, requiredRoles);
                var userName = claimsPrincipal.Identity.Name;

                return new ValidateTokenResult
                {
                    IsValid = isValid,
                    UserName = userName
                };
            }
            catch (InvalidOperationException ex)
            {
                _log.LogMethodError(correlationId, nameof(ValidateTokenAsync), "An invalid operation exception was caught", ex);
                return new ValidateTokenResult
                {
                    IsValid = false,

                };
            }
            catch (SecurityTokenValidationException ex)
            {
                _log.LogMethodError(correlationId, nameof(ValidateTokenAsync), "A security exception was caught", ex);
                return new ValidateTokenResult
                {
                    IsValid = false,
                };
            }
            catch (Exception ex)
            {
                _log.LogMethodError(correlationId, nameof(ValidateTokenAsync), "An unexpected error was caught", ex);
                return new ValidateTokenResult
                {
                    IsValid = false,
                };
            }
        }

        private bool IsValid(ClaimsPrincipal claimsPrincipal, string scopes = null, string roles = null)
        {
            if (claimsPrincipal == null)
            {
                return false;
            }

            var requiredScopes = LoadRequiredItems(scopes);
            var requiredRoles = LoadRequiredItems(roles);

            if (!requiredScopes.Any() && !requiredRoles.Any())
            {
                return true;
            }

            var hasAccessToRoles = !requiredRoles.Any() || requiredRoles.All(claimsPrincipal.IsInRole);
            var scopeClaim = claimsPrincipal.HasClaim(x => x.Type == ScopeType)
                ? claimsPrincipal.Claims.First(x => x.Type == ScopeType).Value
                : string.Empty;

            var tokenScopes = scopeClaim.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var hasAccessToScopes = !requiredScopes.Any() || requiredScopes.All(x => tokenScopes.Any(y => string.Equals(x, y, StringComparison.OrdinalIgnoreCase)));

            return hasAccessToRoles && hasAccessToScopes;
        }

        private static List<string> LoadRequiredItems(string items)
        {
            return string.IsNullOrWhiteSpace(items)
                ? new List<string>()
                : items.Replace(" ", string.Empty).Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}
