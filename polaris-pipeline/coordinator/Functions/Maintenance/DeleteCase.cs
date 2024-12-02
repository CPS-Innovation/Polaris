using Common.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using coordinator.Durable.Providers;
using coordinator.Services.ClearDownService;
using Common.Extensions;
using Microsoft.AspNetCore.Http;
using coordinator.Helpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;

namespace coordinator.Functions.Maintenance
{
    public class DeleteCase
    {
        private readonly ILogger<DeleteCase> _logger;
        private readonly IOrchestrationProvider _orchestrationProvider;
        private readonly IClearDownService _clearDownService;

        public DeleteCase(
            ILogger<DeleteCase> logger,
            IOrchestrationProvider orchestrationProvider,
            IClearDownService clearDownService)
        {
            _logger = logger;
            _orchestrationProvider = orchestrationProvider;
            _clearDownService = clearDownService;
        }

        [Function(nameof(DeleteCase))]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Run
            (
                [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = RestApi.Case)] HttpRequest req,
                string caseUrn,
                int caseId,
                [DurableClient] DurableTaskClient orchestrationClient
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

                await _clearDownService.DeleteCaseAsync(orchestrationClient,
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