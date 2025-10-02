// <copyright file="CaseInfoService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services;

using System;
using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.Interfaces.Model;
using Microsoft.Extensions.Logging;
using System.Globalization;
using Cps.Fct.Hk.Ui.Interfaces;
using Common.Dto.Response.HouseKeeping;
using Common.Dto.Request.HouseKeeping;
using DdeiClient.Clients.Interfaces;

/// <summary>
/// Provides services for retrieving information related to a case.
/// </summary>
public class CaseInfoService(
    ILogger<CaseInfoService> logger,
    IMasterDataServiceClient mdsClient)
    : ICaseInfoService
{
    private readonly ILogger<CaseInfoService> logger = logger;
    private readonly IMasterDataServiceClient mdsClient = mdsClient;

    /// <inheritdoc />
    public async Task<CaseSummaryResponse> GetCaseInfoAsync(int caseId, Common.Dto.Request.CmsAuthValues cmsAuthValues)
    {
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);

        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Fetching info for caseId [{caseIdString}] ...");

            var request = new GetCaseSummaryRequest(caseId, Guid.NewGuid());

            CaseSummaryResponse? caseSummary = await this.mdsClient.GetCaseSummaryAsync(request, cmsAuthValues).ConfigureAwait(false);

            return caseSummary ?? throw new InvalidOperationException($"{LoggingConstants.HskUiLogPrefix} No case summary found for caseId [{caseIdString}]");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching case information for caseId [{caseIdString}]");
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }
}
