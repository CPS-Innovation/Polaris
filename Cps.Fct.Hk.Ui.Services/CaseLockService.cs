// <copyright file="CaseLockService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Globalization;
using Cps.Fct.Hk.Ui.Interfaces;
using Common.Dto.Response.HouseKeeping;
using Common.Dto.Request;
using Common.Constants;
using DdeiClient.Clients;
using DdeiClient.Clients.Interfaces;

/// <summary>
/// Provides services for unlocking a case.
/// </summary>
public class CaseLockService(
    ILogger<CaseLockService> logger,
    IMasterDataServiceClient apiClient)
    : ICaseLockService
{
    private readonly ILogger<CaseLockService> logger = logger;
    private readonly IMasterDataServiceClient apiClient = apiClient;

    /// <inheritdoc />
    public async Task<CaseLockedStatusResult> CheckCaseLockAsync(int caseId, CmsAuthValues cmsAuthValues)
    {
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);

        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Attempting to check case lock status for caseId [{caseIdString}]");

            var checkCaseLockStatusResponse = await this.apiClient.CheckCaseLockAsync(caseId, cmsAuthValues).ConfigureAwait(false);

            this.logger.LogInformation(LoggingConstants.CheckCaseLockOperationSuccess, LoggingConstants.HskUiLogPrefix, caseIdString);

            return checkCaseLockStatusResponse;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, LoggingConstants.CheckCaseLockOperationFailed, LoggingConstants.HskUiLogPrefix, caseIdString);
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }
}
