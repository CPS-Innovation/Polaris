// <copyright file="VerifyMsService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Cps.Fct.Hk.Ui.Interfaces;
using Cps.Fct.Hk.Ui.Interfaces.Model;
using System.Security.Claims;

/// <summary>
/// Provides services for handling the Microsoft Entra ID token.
/// </summary>
public class VerifyMsService(ILogger<VerifyMsService> logger, IJwtValidator jwtValidator, IJwtClaimsExtractor jwtClaimsExtractor)
    : IVerifyMsService
{
    private const string NameClaimType = "name";
    private const string UserNameClaimType = "preferred_username";
    private readonly ILogger<VerifyMsService> logger = logger;
    private readonly IJwtValidator jwtValidator = jwtValidator;
    private readonly IJwtClaimsExtractor jwtClaimsExtractor = jwtClaimsExtractor;

    /// <inheritdoc />
    public async Task<VerifyMsResult> ProcessRequest(string? idToken)
    {
        this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Verifying idToken ...");

        if (string.IsNullOrWhiteSpace(idToken))
        {
            return new VerifyMsResult
            {
                Status = VerifyMsResultStatus.BadRequest,
                Message = "id_token is null or blank.",
            };
        }

        try
        {
            bool isValid = await this.jwtValidator.ValidateJwtSignatureAsync(idToken).ConfigureAwait(true);

            if (isValid)
            {
                return new VerifyMsResult
                {
                    Status = VerifyMsResultStatus.OK,
                    Message = $"Valid id_token: {idToken}",
                };
            }
            else
            {
                return new VerifyMsResult
                {
                    Status = VerifyMsResultStatus.Unauthorized,
                    Message = "id_token is invalid.",
                };
            }
        }
        catch (ArgumentException ex)
        {
            this.logger.LogError(ex, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public void ExtractAndLogUserClaims(string idToken, int? caseId)
    {
        if (string.IsNullOrEmpty(idToken))
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} id_token is null or blank");
            return;
        }

        try
        {
            IEnumerable<Claim> claims = this.jwtClaimsExtractor.ExtractClaims(idToken);

            string? nameClaim = claims.FirstOrDefault(c => c.Type == NameClaimType)?.Value;

            string? userNameClaim = claims.FirstOrDefault(c => c.Type == UserNameClaimType)?.Value;

            if (string.IsNullOrWhiteSpace(nameClaim))
            {
                this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} claim value is null or blank for claimType [{NameClaimType}] for id_token");
            }

            if (string.IsNullOrWhiteSpace(userNameClaim))
            {
                this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} claim value is null or blank for claimType [{UserNameClaimType}] for id_token");
            }

            // Log user logged in successfully.
            this.logger.LogInformation(LoggingConstants.UserLogInSuccess, LoggingConstants.HskUiLogPrefix, userNameClaim, nameClaim, caseId);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, ex.Message);
        }
    }
}
