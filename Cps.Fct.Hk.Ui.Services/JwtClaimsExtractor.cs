// <copyright file="JwtClaimsExtractor.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Cps.Fct.Hk.Ui.Interfaces;
using Cps.Fct.Hk.Ui.Interfaces.Model;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provides implementation for extracting claims from jwt token.
/// </summary>
/// <param name="logger">The logger instance used for logging information and errors.</param>
public class JwtClaimsExtractor(ILogger<JwtClaimsExtractor> logger)
    : IJwtClaimsExtractor
{
    private readonly ILogger<JwtClaimsExtractor> logger = logger;

    /// <inheritdoc/>
    public IEnumerable<Claim> ExtractClaims(string jwtToken)
    {
        this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Extracting user claims from id_token...");

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // Reads token and extract list of claims from it.
            var securityToken = (JwtSecurityToken)tokenHandler.ReadToken(jwtToken);

            // Claims is not null here.
            return securityToken.Claims;
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} Claims extraction failed for id_token: {ex.Message}");
            return Enumerable.Empty<Claim>();
        }
    }
}
