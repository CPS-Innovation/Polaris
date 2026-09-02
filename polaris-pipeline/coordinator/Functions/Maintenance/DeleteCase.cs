// <copyright file="DeleteCase.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Functions.Maintenance;

using Common.Configuration;
using Common.Dto.Request;
using Common.Extensions;
using coordinator.Services.ClearDownService;
using DdeiClient.Services.CaseUrnResolver;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using System.Threading;
using System.Threading.Tasks;

public class DeleteCase(IClearDownService clearDownService)
{
    [Function(nameof(DeleteCase))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = RestApi.Case)] HttpRequest req,
            int caseId,
            CancellationToken cancellationToken,
            [DurableClient] DurableTaskClient orchestrationClient)
    {
        var currentCorrelationId = req.Headers.GetCorrelationId();

        await clearDownService.DeleteCaseAsync(
            orchestrationClient,
            null,
            caseId,
            currentCorrelationId,
            isLegacy: false);

        return new AcceptedResult();
    }
}
