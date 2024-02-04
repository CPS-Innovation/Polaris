using System;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Dto.Case;
using Common.Logging;
using Common.Validators.Contracts;
using Ddei.Exceptions;
using Ddei.Factories.Contracts;
using DdeiClient.Services.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using PolarisGateway.Extensions;
using Common.Telemetry.Wrappers.Contracts;

namespace PolarisGateway.Functions.CaseData
{
    public class Case : BasePolarisFunction
    {
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _caseDataArgFactory;
        private readonly ILogger<Case> _logger;

        const string loggingName = $"{nameof(Case)} - {nameof(Run)}";

        public Case(ILogger<Case> logger,
                    IDdeiClient ddeiService,
                    IAuthorizationValidator tokenValidator,
                    IDdeiArgFactory caseDataArgFactory,
                    ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
            : base(logger, tokenValidator, telemetryAugmentationWrapper)
        {
            _ddeiClient = ddeiService;
            _caseDataArgFactory = caseDataArgFactory;
            _logger = logger;
        }

        [FunctionName(nameof(Case))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Case)] HttpRequest req, string caseUrn, int caseId)
        {
            Guid currentCorrelationId = default;
            CaseDto caseDetails = null;

            try
            {
                var validationResult = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                if (validationResult.InvalidResponseResult != null)
                    return validationResult.InvalidResponseResult;

                currentCorrelationId = validationResult.CurrentCorrelationId;
                var cmsAuthValues = validationResult.CmsAuthValues;

                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Getting case details by Id {caseId}");
                var caseArg = _caseDataArgFactory.CreateCaseArg(cmsAuthValues, currentCorrelationId, caseUrn, caseId);
                caseDetails = await _ddeiClient.GetCaseAsync(caseArg);

                return caseDetails != null
                    ? new OkObjectResult(caseDetails)
                    : NotFoundErrorResponse($"No data found for case id '{caseId}'.", currentCorrelationId, loggingName);
            }
            catch (Exception exception)
            {
                return exception switch
                {
                    MsalException => InternalServerErrorResponse(exception, "An MSAL exception occurred.", currentCorrelationId, loggingName),
                    CaseDataServiceException => CmsAuthValuesErrorResponse(exception.Message, currentCorrelationId, loggingName),
                    _ => InternalServerErrorResponse(exception, "An unhandled exception occurred.", currentCorrelationId, loggingName)
                };
            }
            finally
            {
                _logger.LogMethodExit(currentCorrelationId, loggingName, caseDetails.ToJson());
            }
        }
    }
}

