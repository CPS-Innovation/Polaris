using System;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Dto.Case;
using Common.Logging;
using Common.Validators.Contracts;
using Ddei.Exceptions;
using Ddei.Factories.Contracts;
using Ddei.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using PolarisGateway.Extensions;
using PolarisGateway.Services;
using PolarisGateway.Wrappers;

namespace PolarisGateway.Functions.CaseData
{
    public class CaseDataApiCaseDetails : BasePolarisFunction
    {
        private readonly ICaseDataService _caseDataService;
        private readonly ICaseDataArgFactory _caseDataArgFactory;
        private readonly ILogger<CaseDataApiCaseDetails> _logger;

        const string loggingName = $"{nameof(CaseDataApiCaseDetails)} - {nameof(Run)}";

        public CaseDataApiCaseDetails(ILogger<CaseDataApiCaseDetails> logger,
                                      ICaseDataService caseDataService,
                                      IAuthorizationValidator tokenValidator,
                                      ICaseDataArgFactory caseDataArgFactory,
                                      IOptions<DdeiOptions> options,
                                      ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
        : base(logger, tokenValidator, telemetryAugmentationWrapper)
        {
            _caseDataService = caseDataService;
            _caseDataArgFactory = caseDataArgFactory;
            _logger = logger;
        }

        [FunctionName(nameof(CaseDataApiCaseDetails))]
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
                caseDetails = await _caseDataService.GetCase(_caseDataArgFactory.CreateCaseArg(cmsAuthValues, currentCorrelationId, caseUrn, caseId));

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

