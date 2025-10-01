// <copyright file="UmaReclassifyService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services;

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Cps.Fct.Hk.Ui.Interfaces;
using Cps.Fct.Hk.Ui.ServiceClient.Uma;
using Cps.Fct.Hk.Ui.Interfaces.Model;

/// <summary>
/// Service for handling reclassification requests using unused materials automation.
/// </summary>
public class UmaReclassifyService(ILogger<UmaReclassifyService> logger, IUmaServiceClient umaClient)
    : IUmaReclassifyService
{
    private readonly ILogger<UmaReclassifyService> logger = logger;
    private readonly IUmaServiceClient umaClient = umaClient;

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<MatchedCommunication>> ProcessMatchingRequest(int caseId, IReadOnlyCollection<Communication> communications)
    {
        ArgumentNullException.ThrowIfNull(communications);

        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} ProcessMatchingRequest ...");
            IReadOnlyCollection<MatchedCommunication> matchedCommunications = await this.umaClient.MatchCommunicationsUmAsync(caseId, communications).ConfigureAwait(false);
            return matchedCommunications;
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} ProcessMatchingRequest encountered an error: {ex.Message}");
            throw;
        }
    }
}
