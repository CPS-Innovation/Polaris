﻿using Common.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using coordinator.Durable.Providers;
using coordinator.Services.CleardownService;
using Common.Extensions;
using Microsoft.AspNetCore.Http;
using coordinator.Helpers;

namespace coordinator.Functions.Maintenance
{
    public class DeleteCase
    {
        private readonly ILogger<DeleteCase> _logger;
        private readonly IOrchestrationProvider _orchestrationProvider;
        private readonly ICleardownService _cleardownService;

        public DeleteCase(
            ILogger<DeleteCase> logger,
            IOrchestrationProvider orchestrationProvider,
            ICleardownService cleardownService)
        {
            _logger = logger;
            _orchestrationProvider = orchestrationProvider;
            _cleardownService = cleardownService;
        }

        [FunctionName(nameof(DeleteCase))]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Run
            (
                [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = RestApi.Case)] HttpRequest req,
                string caseUrn,
                int caseId,
                [DurableClient] IDurableOrchestrationClient orchestrationClient
            )
        {
            Guid currentCorrelationId = default;

            try
            {
                try
                {
                    currentCorrelationId = req.Headers.GetCorrelationId();
                }
                catch (Exception)
                {
                    // this is a maintenance function, so we don't want to fail the request if the correlationId is missing
                    currentCorrelationId = Guid.NewGuid();
                }

                await _cleardownService.DeleteCaseAsync(orchestrationClient,
                     caseUrn,
                     caseId,
                     currentCorrelationId);

                return new AcceptedResult();
            }
            catch (Exception ex)
            {
                return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(DeleteCase), currentCorrelationId, ex);
            }
        }
    }
}