using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Dto.Case;
using Common.Extensions;
using Common.Logging;
using Common.Validators.Contracts;
using Ddei.Exceptions;
using Ddei.Factories.Contracts;
using Ddei.Options;
using DdeiClient.Services.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using PolarisGateway.Extensions;
using PolarisGateway.Wrappers;

namespace PolarisGateway.Functions.CaseData
{
    public class CaseDataApiCaseInformationByUrn : BasePolarisFunction
    {
        private readonly IDdeiClient _caseDataService;
        private readonly ICaseDataArgFactory _caseDataArgFactory;
        private readonly ILogger<CaseDataApiCaseInformationByUrn> _logger;
        private readonly DdeiOptions _ddeiOptions;

        const string loggingName = $"{nameof(CaseDataApiCaseInformationByUrn)} - {nameof(Run)}";

        public CaseDataApiCaseInformationByUrn(ILogger<CaseDataApiCaseInformationByUrn> logger,
                                               IDdeiClient caseDataService,
                                               IAuthorizationValidator tokenValidator,
                                               ICaseDataArgFactory caseDataArgFactory,
                                               IOptions<DdeiOptions> options,
                                               ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
        : base(logger, tokenValidator, telemetryAugmentationWrapper)
        {
            _caseDataService = caseDataService;
            _caseDataArgFactory = caseDataArgFactory;
            _logger = logger;
            _ddeiOptions = options.Value;
        }

        [FunctionName(nameof(CaseDataApiCaseInformationByUrn))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Cases)] HttpRequest req, string caseUrn)
        {
            Guid currentCorrelationId = default;
            IEnumerable<CaseDto> caseInformation = null;

            try
            {
                var validationResult = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                if (validationResult.InvalidResponseResult != null)
                    return validationResult.InvalidResponseResult;

                currentCorrelationId = validationResult.CurrentCorrelationId;
                var cmsAuthValues = validationResult.CmsAuthValues;

                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                if (string.IsNullOrEmpty(caseUrn))
                    return BadRequestErrorResponse("Urn is not supplied.", currentCorrelationId, loggingName);

                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Getting case information by Urn '{caseUrn}'");
                var urnArg = _caseDataArgFactory.CreateUrnArg(cmsAuthValues, currentCorrelationId, caseUrn);
                caseInformation = await _caseDataService.ListCases(urnArg);

                if (caseInformation != null && caseInformation.Any())
                {
                    return new OkObjectResult(caseInformation);
                }

                return NotFoundErrorResponse($"No data found for urn '{caseUrn}'.", currentCorrelationId, loggingName);
            }
            catch (Exception exception)
            {
                return exception switch
                {
                    MsalException => InternalServerErrorResponse(exception, $"An MSAL exception occurred. {exception.NestedMessage()}", currentCorrelationId, loggingName),
                    CaseDataServiceException => CmsAuthValuesErrorResponse(exception.NestedMessage(), currentCorrelationId, loggingName),
                    _ => InternalServerErrorResponse(exception, $"An unhandled exception occurred. {exception.NestedMessage()}", currentCorrelationId, loggingName)
                };
            }
            finally
            {
                _logger.LogMethodExit(currentCorrelationId, loggingName, caseInformation.ToJson());
            }
        }
    }
}

