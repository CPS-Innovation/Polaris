// <copyright file="ReclassificationService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services;
using System;
using System.Threading.Tasks;
using Common.Constants;
using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response.HouseKeeping;
using Cps.Fct.Hk.Ui.Interfaces;
using DdeiClient.Clients.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
///  Provides a service for reclassifying case materials related to a case.
/// </summary>
public class ReclassificationService(
      ILogger<ReclassificationService> logger,
      IMasterDataServiceClient apiClient)
    : IReclassificationService
{
    private readonly ILogger<ReclassificationService> logger = logger;
    private readonly IMasterDataServiceClient apiClient = apiClient;

    /// <inheritdoc/>
    public async Task<ReclassificationResponse> ReclassifyCaseMaterialAsync(
        ReclassifyCaseMaterialServiceRequest request,
        CmsAuthValues cmsAuthValues,
        CancellationToken cancellationToken = default)
    {
        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Attempting to reclassify a case material with materidId [{request.MaterialId}] associated with case with caseId [{request.CaseId}]");

            // Handle an exception to the rule for 'Defence statement' category that requires a different classification.
            var classification = request.DocumentTypeId == -2 ? "DEFENCESTATEMENT" : request.Classification;

            var communicationRequest = new ReclassifyCommunicationRequest(
                request.CorrespondenceId != default ? request.CorrespondenceId : Guid.NewGuid(),
                classification,
                request.MaterialId,
                request.DocumentTypeId,
                request.Used,
                request.Subject,
                request.Statement,
                request.Exhibit);

            ReclassificationResponse reclassificationResponse = await this.apiClient.ReclassifyCommunicationAsync(communicationRequest, cmsAuthValues, cancellationToken).ConfigureAwait(false);
            this.logger.LogInformation(LoggingConstants.ReclassifyCaseMaterialOperationSuccess, LoggingConstants.HskUiLogPrefix, request.CaseId, request.MaterialId);

            return reclassificationResponse;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, LoggingConstants.ReclassifyCaseMaterialOperationFailed, LoggingConstants.HskUiLogPrefix, request.CaseId, request.MaterialId);
            throw;
        }
    }
}
