using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using PolarisGateway.Domain.Validation;

namespace Common.Validators.Contracts
{
    public interface IAuthorizationValidator
    {
        Task<ValidateTokenResult> ValidateTokenAsync(StringValues token, Guid correlationId, string requiredScopes = null, string requiredRoles = null);
    }
}
