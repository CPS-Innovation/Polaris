// <copyright file="ReclassificationService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services;
using System;
using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.Interfaces;
using Cps.Fct.Hk.Ui.Interfaces.Model;
using Cps.Fct.Hk.Ui.ServiceClient.Ddei;
using Cps.Fct.Hk.Ui.ServiceClient.Ddei.Model;
using Microsoft.Extensions.Logging;

/// <summary>
///  Provides a service for reclassifying case materials related to a case.
/// </summary>
public class ReclassificationService(
      ILogger<ReclassificationService> logger,
      IDdeiServiceClient apiClient)
    : IReclassificationService
{
    private readonly ILogger<ReclassificationService> logger = logger;
    private readonly IDdeiServiceClient apiClient = apiClient;

    /// <inheritdoc/>
    public async Task<ReclassificationResponse> ReclassifyCaseMaterialAsync(int caseId, int materialId, string classification, int documentTypeId, bool used, string subject, CmsAuthValues cmsAuthValues, ReclassifyStatementRequest? statement = null, ReclassifyExhibitRequest? exhibit = null, Guid correspondenceId = default)
    {
        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Attempting to reclassify a case material with materidId [{materialId}] associated with case with caseId [{caseId}]");

            // Handle an exception to the rule for 'Defence statement' category that requires a different classification.
            if (documentTypeId == -2)
            {
                classification = "DEFENCESTATEMENT";
            }

            var request = new ReclassifyCommunicationRequest(correspondenceId != default ? correspondenceId : Guid.NewGuid(), classification, materialId, documentTypeId, used, subject, statement, exhibit);

            ReclassificationResponse reclassificationResponse = await this.apiClient.ReclassifyCommunicationAsync(request, cmsAuthValues).ConfigureAwait(false);
            this.logger.LogInformation(LoggingConstants.ReclassifyCaseMaterialOperationSuccess, LoggingConstants.HskUiLogPrefix, caseId, materialId);

            return reclassificationResponse;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, LoggingConstants.ReclassifyCaseMaterialOperationFailed, LoggingConstants.HskUiLogPrefix, caseId, materialId);
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }
}
