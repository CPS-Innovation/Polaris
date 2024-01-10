using Microsoft.Extensions.Primitives;
using PolarisGateway.Domain.Validation;

namespace polaris_common.Validators.Contracts
{
    public interface IAuthorizationValidator
    {
        Task<ValidateTokenResult> ValidateTokenAsync(StringValues token, Guid correlationId, string requiredScopes = null, string requiredRoles = null);
    }
}
