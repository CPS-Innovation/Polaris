using System;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Extensions;
using coordinator.Helpers;
using Ddei.Factories;
using Ddei;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions
{
    public class GetWitnessStatements
    {
        private readonly ILogger<GetWitnessStatements> _logger;
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;

        public GetWitnessStatements(ILogger<GetWitnessStatements> logger, IDdeiClient ddeiClient, IDdeiArgFactory ddeiArgFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
            _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
        }

        [FunctionName(nameof(GetWitnessStatements))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.WitnessStatements)] HttpRequest req,
            string caseUrn,
            int caseId,
            int witnessId)
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();
                var cmsAuthValues = req.Headers.GetCmsAuthValues();

                var arg = _ddeiArgFactory.CreateWitnessStatementsArgDto(cmsAuthValues, currentCorrelationId, caseUrn, caseId, witnessId);
                var result = await _ddeiClient.GetWitnessStatementsAsync(arg);

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(GetWitnessStatements), currentCorrelationId, ex);
            }
        }
    }
}