using Common.Configuration;
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
    public class Case : BasePolarisFunction
    {
        private readonly IDdeiClient _ddeiClient;
        private readonly ICaseDataArgFactory _caseDataArgFactory;

        public Case(ILogger<Case> logger,
                    IDdeiClient ddeiService,
                    IAuthorizationValidator tokenValidator,
                    ICaseDataArgFactory caseDataArgFactory,
                    ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
            : base(logger, tokenValidator, telemetryAugmentationWrapper)
        {
            _ddeiClient = ddeiService;
            _caseDataArgFactory = caseDataArgFactory;
        }

        [FunctionName(nameof(Case))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)] // is this useful? does this clash with the 404 if we hit asn unknown endpoint on a function app?
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Case)] HttpRequest req, string caseUrn, int caseId)
        {
            Guid currentCorrelationId = default;

            try
            {
                var validationResult = await ValidateRequest(req, nameof(Case), ValidRoles.UserImpersonation);
                if (validationResult.InvalidResponseResult != null)
                    return validationResult.InvalidResponseResult;

                currentCorrelationId = validationResult.CurrentCorrelationId;
                var cmsAuthValues = validationResult.CmsAuthValues;

                var caseArg = _caseDataArgFactory.CreateCaseArg(cmsAuthValues, currentCorrelationId, caseUrn, caseId);
                var caseDetails = await _ddeiClient.GetCaseAsync(caseArg);

                return caseDetails != null
                    ? new OkObjectResult(caseDetails)
                    : NotFoundErrorResponse($"No data found for case id '{caseId}'.", currentCorrelationId, nameof(Case));
            }
            catch (Exception exception)
            {
                return exception switch
                {
                    MsalException => InternalServerErrorResponse(exception, "An MSAL exception occurred.", currentCorrelationId, nameof(Case)),
                    CaseDataServiceException => CmsAuthValuesErrorResponse(exception.Message, currentCorrelationId, nameof(Case)),
                    _ => InternalServerErrorResponse(exception, "An unhandled exception occurred.", currentCorrelationId, nameof(Case))
                };
            }
        }
    }
}

