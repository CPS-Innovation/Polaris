// <copyright file="CaseDefendantsService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services;

using System;
using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.ServiceClient.Ddei;
using Cps.Fct.Hk.Ui.Interfaces.Model;
using Cps.Fct.Hk.Ui.ServiceClient.Ddei.Model;
using Microsoft.Extensions.Logging;
using System.Globalization;
using Cps.Fct.Hk.Ui.Interfaces;

/// <summary>
/// Provides services for retrieving defendants related to a case.
/// </summary>
public class CaseDefendantsService(
    ILogger<CaseDefendantsService> logger,
    IDdeiServiceClient apiClient)
    : ICaseDefendantsService
{
    private readonly ILogger<CaseDefendantsService> logger = logger;
    private readonly IDdeiServiceClient apiClient = apiClient;

    /// <inheritdoc />
    public async Task<DefendantsResponse> GetCaseDefendantsAsync(int caseId, CmsAuthValues cmsAuthValues)
    {
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);

        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Fetching defendants for caseId [{caseIdString}] ...");

            var request = new ListCaseDefendantsRequest(caseId, Guid.NewGuid());

            DefendantsResponse? defendants = await this.apiClient.ListCaseDefendantsAsync(request, cmsAuthValues).ConfigureAwait(false);

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
