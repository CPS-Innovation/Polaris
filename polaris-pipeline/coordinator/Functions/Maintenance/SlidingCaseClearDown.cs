// <copyright file="SlidingCaseClearDown.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Functions.Maintenance;

using Common.Logging;
using coordinator.Constants;
using coordinator.Durable.Providers;
using coordinator.Services.ClearDownService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

public class SlidingCaseClearDown(ILogger<SlidingCaseClearDown> logger, IConfiguration configuration, IOrchestrationProvider orchestrationProvider, IClearDownService clearDownService)
{
    [Function(nameof(SlidingCaseClearDown))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task RunAsync([TimerTrigger("%SlidingClearDownSchedule%")] TimerInfo myTimer, [DurableClient] DurableTaskClient client)
    {
        var correlationId = Guid.NewGuid();

        try
        {
            var hoursBackNumber = double.Parse(configuration[ConfigKeys.SlidingClearDownInputHours]);
            var countCases = int.Parse(configuration[ConfigKeys.SlidingClearDownBatchSize]);
            var earliestDateToKeep = DateTime.UtcNow.AddHours(hoursBackNumber * -1);
            var caseIds = await orchestrationProvider.FindCaseInstancesByDateAsync(client, earliestDateToKeep, countCases);

            // first pass: lets do the cases in sequence rather than parallel, until we are sure of search index characteristics
            foreach (var caseId in caseIds)
            {
                // pass an explicit string for the caseUrn for logging purposes as we don't have access to the caseUrn here
                await clearDownService.DeleteCaseAsync(
                    client,
                    "sliding-clear-down",
                    caseId,
                    correlationId,
                    isLegacy: true,
                    removeBlobs: false);
            }
        }
        catch (Exception ex)
        {
            logger.LogMethodError(correlationId, nameof(SlidingCaseClearDown), ex.Message, ex);
        }
    }
}
