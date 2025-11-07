// <copyright file="CaseActionPlanService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Globalization;
using Cps.Fct.Hk.Ui.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Cps.Fct.Hk.Ui.Interfaces.Exceptions;
using Common.Constants;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Request;
using DdeiClient.Clients.Interfaces;

/// <summary>
/// Provides services for creating and sending action plans related to a case.
/// </summary>
public class CaseActionPlanService(
    ILogger<CaseActionPlanService> logger,
    ICaseLockService caseLockService,
    IMasterDataServiceClient apiClient)
    : ICaseActionPlanService
{
    private readonly ILogger<CaseActionPlanService> logger = logger;
    private readonly ICaseLockService caseLockService = caseLockService;
    private readonly IMasterDataServiceClient apiClient = apiClient;

    /// <inheritdoc />
    public async Task<NoContentResult> AddCaseActionPlanAsync(string urn, int caseId, AddCaseActionPlanRequest addCaseActionPlanRequest, CmsAuthValues cmsAuthValues)
    {
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);

        var checkCaeLockStatus = await this.caseLockService.CheckCaseLockAsync(caseId, cmsAuthValues).ConfigureAwait(false);
        if (checkCaeLockStatus.IsLocked && !checkCaeLockStatus.IsLockedByCurrentUser)
        {
            throw new CaseLockedException($"{LoggingConstants.HskUiLogPrefix} Attempting to add case action plan for caseId [{caseIdString}] failed as case is locked by [{checkCaeLockStatus.LockedByUser}].");
        }

        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Attempting to add case action plan for caseId [{caseIdString}]");

            Step[]? mappedSteps = addCaseActionPlanRequest.steps?
           .Select(s => new Step(
               code: s.code,
               description: s.description,
               text: s.text,
               hidden: s.hidden,
               hiddenDraft: s.hiddenDraft))
           .ToArray();

            var request = new AddActionPlanRequest(
                Id: Guid.NewGuid(),
                fullDefendantName: addCaseActionPlanRequest.fullDefendantName,
                allDefendants: addCaseActionPlanRequest.allDefendants,
                date: addCaseActionPlanRequest.date,
                dateExpected: addCaseActionPlanRequest.dateExpected,
                dateTimeCreated: addCaseActionPlanRequest.dateTimeCreated,
                type: addCaseActionPlanRequest.type,
                actionPointText: addCaseActionPlanRequest.actionPointText,
                status: addCaseActionPlanRequest.status,
                statusDescription: addCaseActionPlanRequest.statusDescription,
                dG6Justification: addCaseActionPlanRequest.dG6Justification,
                createdByOrganisation: addCaseActionPlanRequest.createdByOrganisation,
                expectedDateUpdated: addCaseActionPlanRequest.expectedDateUpdated,
                partyType: addCaseActionPlanRequest.partyType,
                policeChangeReason: addCaseActionPlanRequest.policeChangeReason,
                statusUpdated: addCaseActionPlanRequest.statusUpdated,
                syncedWithPolice: addCaseActionPlanRequest.syncedWithPolice,
                cpsChangeReason: addCaseActionPlanRequest.cpsChangeReason,
                duplicateOriginalMaterial: addCaseActionPlanRequest.duplicateOriginalMaterial,
                material: addCaseActionPlanRequest.material,
                chaserTaskDate: addCaseActionPlanRequest.chaserTaskDate,
                defendantId: addCaseActionPlanRequest.defendantId,
                steps: mappedSteps!);

            NoContentResult addCaseActionPlanResponse = await this.apiClient.AddCaseActionPlanAsync(caseId, request, cmsAuthValues).ConfigureAwait(false);

            this.logger.LogInformation(LoggingConstants.AddCaseActionPlanOperationSuccess, LoggingConstants.HskUiLogPrefix, caseIdString);

            return addCaseActionPlanResponse;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, LoggingConstants.AddCaseActionPlanOperationFailed, LoggingConstants.HskUiLogPrefix, caseIdString);
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }
}
