// <copyright file="CaseDefendantsService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Globalization;
using Cps.Fct.Hk.Ui.Interfaces;
using DdeiClient.Clients.Interfaces;
using Common.Dto.Response.HouseKeeping;
using Common.Dto.Request;
using Common.Constants;
using Common.Dto.Request.HouseKeeping;

/// <summary>
/// Provides services for retrieving defendants related to a case.
/// </summary>
public class CaseDefendantsService(
    ILogger<CaseDefendantsService> logger,
    IMasterDataServiceClient apiClient)
    : ICaseDefendantsService
{
    private readonly ILogger<CaseDefendantsService> logger = logger;
    private readonly IMasterDataServiceClient apiClient = apiClient;

    /// <inheritdoc />
    public async Task<DefendantsResponse> GetCaseDefendantsAsync(int caseId, CmsAuthValues cmsAuthValues)
    {
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);

        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Fetching defendants for caseId [{caseIdString}] ...");

            var request = new ListCaseDefendantsRequest(caseId, Guid.NewGuid());

            DefendantsResponse? defendants = await this.apiClient.GetCaseDefendantsAsync(request, cmsAuthValues).ConfigureAwait(false);

            return defendants ?? throw new InvalidOperationException($"{LoggingConstants.HskUiLogPrefix} No case defendants found for caseId [{caseIdString}]");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching case defendants for caseId [{caseIdString}]");
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }
}
