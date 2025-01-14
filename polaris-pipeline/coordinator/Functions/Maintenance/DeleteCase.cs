using Common.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using coordinator.Durable.Providers;
using coordinator.Services.ClearDownService;
using Common.Extensions;
using Microsoft.AspNetCore.Http;
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
        public async Task<IActionResult> Run(
                [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = RestApi.Case)] HttpRequest req,
                string caseUrn,
                int caseId,
                [DurableClient] DurableTaskClient orchestrationClient)
        {

            var currentCorrelationId = req.Headers.GetCorrelationId();

            await _clearDownService.DeleteCaseAsync(orchestrationClient,
                 caseUrn,
                 caseId,
                 currentCorrelationId);

            return new AcceptedResult();
        }
    }
}