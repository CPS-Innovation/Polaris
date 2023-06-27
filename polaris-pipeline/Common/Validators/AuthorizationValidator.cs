using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Common.Validators.Contracts;
using Common.Logging;
using Common.Constants;
using Common.Extensions;
using PolarisGateway.Domain.Validation;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Common.Validators
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
            _log.LogMethodEntry(correlationId, nameof(ValidateTokenAsync), string.Empty);
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

                var isValid = IsValid(claimsPrincipal, correlationId, requiredScopes, requiredRoles);
                var userName = claimsPrincipal.Identity.Name;

                return new ValidateTokenResult
                {
                    IsValid = isValid,
                    UserName = userName
                };
            }
            catch (InvalidOperationException invalidOperationException)
            {
                _log.LogMethodError(correlationId, nameof(ValidateTokenAsync), "An invalid operation exception was caught", invalidOperationException);
                return new ValidateTokenResult
                {
                    IsValid = false,

                };
            }
            catch (SecurityTokenValidationException securityException)
            {
                _log.LogMethodError(correlationId, nameof(ValidateTokenAsync), "A security exception was caught", securityException);
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
            finally
            {
                _log.LogMethodExit(correlationId, nameof(ValidateTokenAsync), string.Empty);
            }
        }

        private bool IsValid(ClaimsPrincipal claimsPrincipal, Guid correlationId, string scopes = null, string roles = null)
        {
            _log.LogMethodEntry(correlationId, nameof(IsValid), string.Empty);
            if (claimsPrincipal == null)
            {
                _log.LogMethodFlow(correlationId, nameof(IsValid), "Claims Principal not found - returning 'false' indicating an authorization failure");
                return false;
            }

            var requiredScopes = LoadRequiredItems(scopes);
            var requiredRoles = LoadRequiredItems(roles);

            if (!requiredScopes.Any() && !requiredRoles.Any())
            {
                _log.LogMethodFlow(correlationId, nameof(IsValid), "No required scopes or roles found - allowing access - returning");
                return true;
            }

            var hasAccessToRoles = !requiredRoles.Any() || requiredRoles.All(claimsPrincipal.IsInRole);
            var scopeClaim = claimsPrincipal.HasClaim(x => x.Type == ScopeType)
                ? claimsPrincipal.Claims.First(x => x.Type == ScopeType).Value
                : string.Empty;

            var tokenScopes = scopeClaim.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var hasAccessToScopes = !requiredScopes.Any() || requiredScopes.All(x => tokenScopes.Any(y => string.Equals(x, y, StringComparison.OrdinalIgnoreCase)));

            _log.LogMethodExit(correlationId, nameof(IsValid), $"Outcome role and scope checks - hasAccessToRoles: {hasAccessToRoles}, hasAccessToScopes: {hasAccessToScopes}");
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
