using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using PolarisGateway.CaseDataImplementations.Ddei.Options;
using PolarisGateway.Clients.OnBehalfOfTokenClient;
using PolarisGateway.Domain.CaseData;
using PolarisGateway.Domain.Exceptions;
using PolarisGateway.Domain.Logging;
using PolarisGateway.Domain.Validators;
using PolarisGateway.Extensions;
using PolarisGateway.Factories;
using PolarisGateway.Services;

namespace PolarisGateway.Functions.CaseData
{
    public class CaseDataApiCaseDetails : BasePolarisFunction
    {
        private readonly IOnBehalfOfTokenClient _onBehalfOfTokenClient;
        private readonly ICaseDataService _caseDataService;
        private readonly ICaseDataArgFactory _caseDataArgFactory;
        private readonly ILogger<CaseDataApiCaseDetails> _logger;
        private readonly DdeiOptions _ddeiOptions;

        public CaseDataApiCaseDetails(ILogger<CaseDataApiCaseDetails> logger, IOnBehalfOfTokenClient onBehalfOfTokenClient, ICaseDataService caseDataService,
                                 IAuthorizationValidator tokenValidator, ICaseDataArgFactory caseDataArgFactory, IOptions<DdeiOptions> options)
        : base(logger, tokenValidator)
        {
            _onBehalfOfTokenClient = onBehalfOfTokenClient;
            _caseDataService = caseDataService;
            _caseDataArgFactory = caseDataArgFactory;
            _logger = logger;
            _ddeiOptions = options.Value;
        }

        [FunctionName("CaseDataApiCaseDetails")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "urns/{urn}/cases/{caseId}")] HttpRequest req, string urn, int caseId)
        {
            Guid currentCorrelationId = default;
            const string loggingName = "CaseDataApiCaseDetails - Run";
            CaseDetailsFull caseDetails = null;

            try
            {
                urn = WebUtility.UrlDecode(urn).GetUntilOrEmpty(); // todo: inject or move to validator
                var validationResult = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                if (validationResult.InvalidResponseResult != null)
                    return validationResult.InvalidResponseResult;

                currentCorrelationId = validationResult.CurrentCorrelationId;
                var cmsAuthValues = validationResult.CmsAuthValues;

                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                var ddeiScope = _ddeiOptions.DefaultScope;
                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Getting an access token as part of OBO for the following scope {ddeiScope}");
                var onBehalfOfAccessToken = await _onBehalfOfTokenClient.GetAccessTokenAsync(validationResult.AccessTokenValue.ToJwtString(), ddeiScope, currentCorrelationId);

                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Getting case details by Id {caseId}");
                caseDetails = await _caseDataService.GetCase(_caseDataArgFactory.CreateCaseArg(onBehalfOfAccessToken, cmsAuthValues, currentCorrelationId, urn, caseId));

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

