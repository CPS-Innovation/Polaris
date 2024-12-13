﻿using Microsoft.Extensions.Primitives;
using PolarisGateway.Domain.Validation;
using System;
using System.Threading.Tasks;

namespace PolarisGateway.Validators
{
    public interface IAuthorizationValidator
    {
        Task<ValidateTokenResult> ValidateTokenAsync(StringValues token, Guid correlationId, string requiredScopes = null, string requiredRoles = null);
    }
}
