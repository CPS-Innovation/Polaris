using Common.Configuration;
using Common.Dto.Case;
using Common.Extensions;
using PolarisGateway.Domain.Validators;
using Ddei.Exceptions;
using Ddei.Factories.Contracts;
using DdeiClient.Services.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Common.Telemetry.Wrappers.Contracts;

namespace PolarisGateway.Functions.CaseData
{
    public class Cases : BasePolarisFunction
    {
        private readonly IDdeiClient _ddeiClient;
        private readonly ICaseDataArgFactory _caseDataArgFactory;
        private readonly ILogger<Cases> _logger;

        public Cases(ILogger<Cases> logger,
                        IDdeiClient caseDataService,
                        IAuthorizationValidator tokenValidator,
                        ICaseDataArgFactory caseDataArgFactory,
                        ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
        : base(logger, tokenValidator, telemetryAugmentationWrapper)
        {
            _ddeiClient = caseDataService;
            _caseDataArgFactory = caseDataArgFactory;
            _logger = logger;
        }

        [FunctionName(nameof(Cases))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)] // is this useful? does this clash with the 404 if we hit asn unknown endpoint on a function app?
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Cases)] HttpRequest req, string caseUrn)
        {
            Guid currentCorrelationId = default;
            IEnumerable<CaseDto> caseInformation = null;

            try
            {
                var validationResult = await ValidateRequest(req, nameof(Cases), ValidRoles.UserImpersonation);
                if (validationResult.InvalidResponseResult != null)
                    return validationResult.InvalidResponseResult;

                currentCorrelationId = validationResult.CurrentCorrelationId;
                var cmsAuthValues = validationResult.CmsAuthValues;

                if (string.IsNullOrEmpty(caseUrn))
                    return BadRequestErrorResponse("Urn is not supplied.", currentCorrelationId, nameof(Cases));

                var urnArg = _caseDataArgFactory.CreateUrnArg(cmsAuthValues, currentCorrelationId, caseUrn);
                caseInformation = await _ddeiClient.ListCasesAsync(urnArg);

                if (caseInformation != null && caseInformation.Any())
                {
                    return new OkObjectResult(caseInformation);
                }

                return NotFoundErrorResponse($"No data found for urn '{caseUrn}'.", currentCorrelationId, nameof(Cases));
            }
            catch (Exception exception)
            {
                return exception switch
                {
                    MsalException => InternalServerErrorResponse(exception, $"An MSAL exception occurred. {exception.NestedMessage()}", currentCorrelationId, nameof(Cases)),
                    CaseDataServiceException => CmsAuthValuesErrorResponse(exception.NestedMessage(), currentCorrelationId, nameof(Cases)),
                    _ => InternalServerErrorResponse(exception, $"An unhandled exception occurred. {exception.NestedMessage()}", currentCorrelationId, nameof(Cases))
                };
            }
        }
    }
}

