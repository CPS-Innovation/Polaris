using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using coordinator.Helpers;
using Common.Configuration;
using Common.Dto.Case;
using Common.Extensions;
using Common.Wrappers;
using Ddei.Factories;
using DdeiClient.Services;

namespace coordinator.Functions
{
    public class ReorderStatements
    {
        private readonly ILogger<ReorderStatements> _logger;
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public ReorderStatements(ILogger<ReorderStatements> logger, IDdeiClient ddeiClient, IDdeiArgFactory ddeiArgFactory, IJsonConvertWrapper jsonConvertWrapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
            _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
        }

        [FunctionName(nameof(ReorderStatements))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.ReorderStatements)] HttpRequestMessage req,
            string caseUrn,
            int caseId
            )
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();
                var cmsAuthValues = req.Headers.GetCmsAuthValues();

                var content = await req.Content.ReadAsStringAsync();
                var orderedStatements = _jsonConvertWrapper.DeserializeObject<OrderedStatementsDto>(content);

                // TODO: Validate the incoming object...

                var arg = _ddeiArgFactory.CreateReorderStatementsArgDto(cmsAuthValues, currentCorrelationId, caseUrn, caseId, orderedStatements);

                await _ddeiClient.ReorderStatements(arg);

                return new OkResult();
            }
            catch (Exception ex)
            {
                return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(ReorderStatements), currentCorrelationId, ex);
            }
        }
    }
}