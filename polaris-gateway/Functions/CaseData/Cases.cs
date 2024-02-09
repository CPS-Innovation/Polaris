using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Dto.Case;
using Common.Extensions;
using Common.Logging;
using PolarisGateway.Domain.Validators.Contracts;
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
    public class Cases : BasePolarisFunction
    {
        private readonly IDdeiClient _caseDataService;
        private readonly ICaseDataArgFactory _caseDataArgFactory;
        private readonly ILogger<Cases> _logger;
        const string loggingName = $"{nameof(Cases)} - {nameof(Run)}";

        public Cases(ILogger<Cases> logger,
                        IDdeiClient caseDataService,
                        IAuthorizationValidator tokenValidator,
                        ICaseDataArgFactory caseDataArgFactory,
                        ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
        : base(logger, tokenValidator, telemetryAugmentationWrapper)
        {
            _caseDataService = caseDataService;
            _caseDataArgFactory = caseDataArgFactory;
            _logger = logger;
        }

        [FunctionName(nameof(Cases))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Cases)] HttpRequest req, string caseUrn)
        {
            Guid currentCorrelationId = default;
            IEnumerable<CaseDto> caseInformation = null;

            try
            {
                #region Validate-Inputs
                var validationResult = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                if (validationResult.InvalidResponseResult != null)
                    return validationResult.InvalidResponseResult;

                currentCorrelationId = validationResult.CurrentCorrelationId;
                var cmsAuthValues = validationResult.CmsAuthValues;

                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                if (string.IsNullOrEmpty(caseUrn))
                    return BadRequestErrorResponse("Urn is not supplied.", currentCorrelationId, loggingName);
                #endregion

                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Getting case information by Urn '{caseUrn}'");
                var urnArg = _caseDataArgFactory.CreateUrnArg(cmsAuthValues, currentCorrelationId, caseUrn);
                caseInformation = await _caseDataService.ListCasesAsync(urnArg);

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

