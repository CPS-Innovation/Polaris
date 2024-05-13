using System;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Logging;
using coordinator.Durable.Providers;
using coordinator.Services.CleardownService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.Maintenance;

public class DeleteCase
{
    private readonly ILogger<DeleteCase> _logger;
    private readonly IOrchestrationProvider _orchestrationProvider;
    private readonly ICleardownService _cleardownService;

    public DeleteCase(ILogger<DeleteCase> logger, IOrchestrationProvider orchestrationProvider, ICleardownService cleardownService)
    {
        _logger = logger;
        _orchestrationProvider = orchestrationProvider;
        _cleardownService = cleardownService;
    }

    [FunctionName(nameof(DeleteCase))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = RestApi.Case)][DurableClient] IDurableOrchestrationClient client,
        HttpRequest req,
        string caseUrn,
        int caseId)
    {
        var correlationId = Guid.NewGuid();
        try
        {
            // pass an explicit string for the caseUrn for logging purposes as we don't have access to the caseUrn here
            await _cleardownService.DeleteCaseAsync(client,
             caseUrn,
             caseId,
             correlationId,
             waitForIndexToSettle: false);
        }
        catch (Exception ex)
        {
            _logger.LogMethodError(correlationId, nameof(DeleteCase), ex.Message, ex);
        }
    }
}