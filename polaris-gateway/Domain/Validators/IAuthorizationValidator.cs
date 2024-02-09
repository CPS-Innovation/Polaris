using Microsoft.Extensions.Primitives;
using PolarisGateway.Domain.Validation;

namespace PolarisGateway.Domain.Validators
{
    public interface IAuthorizationValidator
    {
        Task<ValidateTokenResult> ValidateTokenAsync(StringValues token, Guid correlationId, string requiredScopes = null, string requiredRoles = null);
    }
}
