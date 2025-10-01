// <copyright file="WitnessService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.Interfaces;
using Cps.Fct.Hk.Ui.Interfaces.Exceptions;
using Cps.Fct.Hk.Ui.Interfaces.Model;
using Cps.Fct.Hk.Ui.ServiceClient.Ddei;
using Cps.Fct.Hk.Ui.ServiceClient.Ddei.Model;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provides witness related services to a case.
/// </summary>
public class WitnessService(
    ILogger<WitnessService> logger,
    IDdeiServiceClient apiClient,
    ICaseLockService caseLockService)
    : IWitnessService
{
    private readonly ILogger<WitnessService> logger = logger;
    private readonly IDdeiServiceClient apiClient = apiClient;
    private readonly ICaseLockService caseLockService = caseLockService;

    /// <inheritdoc/>
    public async Task<StatementsForWitnessResponse> GetStatementsForWitnessAsync(int witnessId, CmsAuthValues cmsAuthValues)
    {
        string witnessIdString = witnessId.ToString(CultureInfo.InvariantCulture);

        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Getting statements for witnessId [{witnessIdString}]");

            var request = new GetStatementsForWitnessRequest(witnessId, Guid.NewGuid());

            StatementsForWitnessResponse? statements = await this.apiClient.GetStatementsForWitnessAsync(request, cmsAuthValues).ConfigureAwait(false);

            return statements;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching statements for witnessId [{witnessIdString}]");
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<WitnessesResponse> GetWitnessesForCaseAsync(int caseId, CmsAuthValues cmsAuthValues)
    {
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);

        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Getting witnesses for caseId [{caseIdString}]");

            var request = new GetWitnessesForCaseRequest(caseId, Guid.NewGuid());
            WitnessesResponse? witnesses = await this.apiClient.GetWitnessesForCaseAsync(request, cmsAuthValues).ConfigureAwait(false);

            return witnesses;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching witnesses for caseId [{caseIdString}]");
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<int?> AddWitnessAsync(string urn, int caseId, string firstName, string lastName, CmsAuthValues cmsAuthValues, Guid correspondenceId = default)
    {
        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Attempting to add witness to case with a caseId [{caseId}]");

            var checkCaeLockStatus = await this.caseLockService.CheckCaseLockAsync(caseId, cmsAuthValues).ConfigureAwait(false);
            if (checkCaeLockStatus.IsLocked && !checkCaeLockStatus.IsLockedByCurrentUser)
            {
                throw new CaseLockedException($"{LoggingConstants.HskUiLogPrefix} Attempting to add case witness for caseId [{caseId}] is failed as case is locked by [{checkCaeLockStatus.LockedByUser}].");
            }

            int? newWitnessId = null;
            var request = new AddWitnessRequest(correspondenceId == default ? Guid.NewGuid() : correspondenceId, caseId, firstName, lastName);

            await this.apiClient.AddWitnessAsync(request, cmsAuthValues).ConfigureAwait(false);

            // Get the newly added witness details
            WitnessesResponse witnesses = await this.GetWitnessesForCaseAsync(caseId, cmsAuthValues).ConfigureAwait(false);

            Witness? newWitness = witnesses?.Witnesses?
                .Where(x => x.FirstName.Equals(firstName, StringComparison.InvariantCultureIgnoreCase)
                        && x.Surname.Equals(lastName, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault();

            if (newWitness != null && newWitness?.WitnessId > 0)
            {
                newWitnessId = newWitness.WitnessId;
                this.logger.LogInformation(LoggingConstants.AddWitnessOperationSuccess, LoggingConstants.HskUiLogPrefix, caseId, newWitnessId);

                return newWitnessId;
            }
            else
            {
                this.logger.LogError(LoggingConstants.AddWitnessOperationFailed, LoggingConstants.HskUiLogPrefix, caseId);
                throw new InvalidOperationException($"{LoggingConstants.HskUiLogPrefix} Error occurred while adding witness for caseId [{caseId}]");
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, LoggingConstants.AddWitnessOperationFailed, LoggingConstants.HskUiLogPrefix, caseId);
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }
}
