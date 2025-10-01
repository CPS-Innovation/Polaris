// <copyright file="IJwtClaimsExtractor.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces;

using System.Collections.Generic;
using System.Security.Claims;

/// <summary>
/// A jwt helper implementation to allow claims to be extracted from provided jwt token.
/// </summary>
public interface IJwtClaimsExtractor
{
    /// <summary>
    /// Extracts claims from provided jwt token.
    /// </summary>
    /// <param name="jwtToken">The Jwt token.</param>
    /// <returns>Collection of claims.</returns>
    IEnumerable<Claim> ExtractClaims(string jwtToken);
}
