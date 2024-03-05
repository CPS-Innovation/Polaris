using Common.Configuration;
using Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using coordinator.Durable.Providers;
using coordinator.Services.CleardownService;
using coordinator.Durable.Payloads;
using Microsoft.AspNetCore.Http;
using coordinator.Helpers;
using coordinator.Domain;

namespace coordinator.Functions
{
    public class RefreshCase
    {
        private readonly ILogger<RefreshCase> _logger;
        private readonly IOrchestrationProvider _orchestrationProvider;
        private readonly ICleardownService _cleardownService;

        public RefreshCase(
            ILogger<RefreshCase> logger,
            IOrchestrationProvider orchestrationProvider,
            ICleardownService cleardownService)
        {
            _logger = logger;
            _orchestrationProvider = orchestrationProvider;
            _cleardownService = cleardownService;
        }

        [FunctionName(nameof(RefreshCase))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status423Locked)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Run
            (
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.Case)] HttpRequest req,
                string caseUrn,
                int caseId,
                [DurableClient] IDurableOrchestrationClient orchestrationClient
            )
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();
                var cmsAuthValues = req.Headers.GetCmsAuthValues();

                var casePayload = new CaseOrchestrationPayload(caseUrn, caseId, cmsAuthValues, currentCorrelationId);
                var isAccepted = await _orchestrationProvider.RefreshCaseAsync(orchestrationClient, currentCorrelationId, caseId.ToString(), casePayload, req);

                return new ObjectResult(new RefreshCaseResponse())
                {
                    StatusCode = isAccepted
                        ? StatusCodes.Status200OK
                        : StatusCodes.Status423Locked
                };
            }
            catch (Exception ex)
            {
                return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(RefreshCase), currentCorrelationId, ex);
            }
        }
    }
}