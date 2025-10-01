// <copyright file="IJwtValidator.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces;

using System.Threading.Tasks;

/// <summary>
/// Interface for validating various aspects of a JWT token.
/// </summary>
public interface IJwtValidator
{
    /// <summary>
    /// Validates that the JWT token's signature matches its payload.
    /// </summary>
    /// <param name="idToken">The JWT token to validate.</param>
    /// <returns>
    /// A <see cref="Task{Boolean}"/> representing the asynchronous operation.
    /// Returns <c>true</c> if the signature is valid; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> ValidateJwtSignatureAsync(string idToken);
}
