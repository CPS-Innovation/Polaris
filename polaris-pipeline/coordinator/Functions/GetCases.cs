using Common.Configuration;
using Common.Extensions;
using Common.Logging;
using DdeiClient.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Ddei.Factories;
using System.Threading.Tasks;
using System;
using System.Net.Http;

namespace PolarisGateway.Functions
{
    public class GetCases
    {
        private readonly ILogger<GetCases> _logger;
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;

        public GetCases(ILogger<GetCases> logger,
                        IDdeiClient caseDataService,
                        IDdeiArgFactory ddeiArgFactory)
        {
            _logger = logger;
            _ddeiClient = caseDataService;
            _ddeiArgFactory = ddeiArgFactory;
        }

        [FunctionName(nameof(GetCases))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Cases)] HttpRequestMessage req, string caseUrn)
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();
                var cmsAuthValues = req.Headers.GetCmsAuthValues();

                var arg = _ddeiArgFactory.CreateUrnArg(cmsAuthValues, currentCorrelationId, caseUrn);
                var result = await _ddeiClient.ListCasesAsync(arg);

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogMethodError(currentCorrelationId, nameof(GetCase), ex.Message, ex);
                return new StatusCodeResult(500);
            }
        }
    }
}

