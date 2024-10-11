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
    public class GetExhibitProducers
    {
        private readonly ILogger<GetExhibitProducers> _logger;
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;

        public GetExhibitProducers(ILogger<GetExhibitProducers> logger, IDdeiClient ddeiClient, IDdeiArgFactory ddeiArgFactory)
        {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            _ddeiClient = ddeiClient ?? throw new System.ArgumentNullException(nameof(ddeiClient));
            _ddeiArgFactory = ddeiArgFactory ?? throw new System.ArgumentNullException(nameof(ddeiArgFactory));
        }

        [FunctionName(nameof(GetExhibitProducers))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseExhibitProducers)] HttpRequest req,
            string caseUrn,
            int caseId)
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();
                var cmsAuthValues = req.Headers.GetCmsAuthValues();

                var arg = _ddeiArgFactory.CreateCaseArg(cmsAuthValues, currentCorrelationId, caseUrn, caseId);
                var result = await _ddeiClient.GetExhibitProducers(arg);

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(GetDocumentNotes), currentCorrelationId, ex);
            }
        }
    }
}