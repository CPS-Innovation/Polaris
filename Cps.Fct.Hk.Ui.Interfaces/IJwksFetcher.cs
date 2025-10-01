// <copyright file="IJwksFetcher.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces;

using System.Threading.Tasks;

/// <summary>
/// Interface for fetching JSON Web Key Sets (JWKS) from an identity provider.
/// </summary>
public interface IJwksFetcher
{
    /// <summary>
    /// Fetches the JSON Web Key Set (JWKS) for a specified tenant from the identity provider.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the tenant for which to retrieve the JWKS.</param>
    /// <returns>
    /// A <see cref="Task{String}"/> representing the asynchronous operation.
    /// The result contains the JWKS in JSON format as a string.
    /// </returns>
    Task<string> GetJwksAsync(string tenantId);
}
