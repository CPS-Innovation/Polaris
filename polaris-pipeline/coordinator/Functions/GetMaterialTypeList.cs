using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using coordinator.Helpers;
using Common.Configuration;
using Common.Extensions;
using Ddei.Factories;
using DdeiClient.Services;

namespace coordinator.Functions
{
    public class GetMaterialTypeList
    {
        private readonly ILogger<GetMaterialTypeList> _logger;
        private readonly IDdeiArgFactory _ddeiArgFactory;
        private readonly IDdeiClient _ddeiClient;

        public GetMaterialTypeList(ILogger<GetMaterialTypeList> logger, IDdeiArgFactory ddeiArgFactory, IDdeiClient ddeiClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
            _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
        }

        [FunctionName(nameof(GetMaterialTypeList))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.MaterialTypeList)] HttpRequest req)
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();
                var cmsAuthValues = req.Headers.GetCmsAuthValues();

                var arg = _ddeiArgFactory.CreateMaterialTypeListArgDto(cmsAuthValues, currentCorrelationId);
                var result = await _ddeiClient.GetMaterialTypeListAsync(arg);

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(GetDocumentNotes), currentCorrelationId, ex);
            }
        }
    }
}