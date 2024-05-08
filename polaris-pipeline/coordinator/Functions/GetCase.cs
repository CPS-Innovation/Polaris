using System;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Extensions;
using coordinator.Helpers;
using Ddei.Factories;
using DdeiClient.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace PolarisGateway.Functions
{
    public class GetCase
    {
        private readonly ILogger<GetCase> _logger;
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;

        public GetCase(ILogger<GetCase> logger,
                    IDdeiClient ddeiService,
                    IDdeiArgFactory ddeiArgFactory)
        {
            _logger = logger;
            _ddeiClient = ddeiService;
            _ddeiArgFactory = ddeiArgFactory;
        }

        [FunctionName(nameof(GetCase))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Case)] HttpRequest req,
            string caseUrn,
            int caseId)
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();
                var cmsAuthValues = req.Headers.GetCmsAuthValues();

                var arg = _ddeiArgFactory.CreateCaseArg(cmsAuthValues, currentCorrelationId, caseUrn, caseId);
                var result = await _ddeiClient.GetCaseAsync(arg);

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(GetCase), currentCorrelationId, ex);
            }
        }
    }
}

