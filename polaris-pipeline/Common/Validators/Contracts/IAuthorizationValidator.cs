using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace Common.Validators.Contracts
{
    public interface IAuthorizationValidator
    {
        Task<bool> ValidateTokenAsync(StringValues token, Guid correlationId, string requiredScopes = null, string requiredRoles = null);
    }
}
