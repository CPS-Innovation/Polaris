using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using RumpoleGateway.Clients.OnBehalfOfTokenClient;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RumpoleGateway.CaseDataImplementations.Tde.Options;
using RumpoleGateway.Domain.CaseData;
using RumpoleGateway.Domain.Logging;
using RumpoleGateway.Domain.Validators;
using RumpoleGateway.Extensions;
using RumpoleGateway.Services;
using RumpoleGateway.Factories;
using RumpoleGateway.Domain.Exceptions;
using RumpoleGateway.Helpers.Extension;

namespace RumpoleGateway.Functions.CaseDataApi.Case
{
    public class CaseDataApiCaseDetails : BaseRumpoleFunction
    {
        private readonly IOnBehalfOfTokenClient _onBehalfOfTokenClient;
        private readonly ICaseDataService _caseDataService;
        private readonly ICaseDataArgFactory _caseDataArgFactory;
        private readonly ILogger<CaseDataApiCaseDetails> _logger;
        private readonly TdeOptions _tdeOptions;

        public CaseDataApiCaseDetails(ILogger<CaseDataApiCaseDetails> logger, IOnBehalfOfTokenClient onBehalfOfTokenClient, ICaseDataService caseDataService,
                                 IAuthorizationValidator tokenValidator, ICaseDataArgFactory caseDataArgFactory, IOptions<TdeOptions> options)
        : base(logger, tokenValidator)
        {
            _onBehalfOfTokenClient = onBehalfOfTokenClient;
            _caseDataService = caseDataService;
            _caseDataArgFactory = caseDataArgFactory;
            _logger = logger;
            _tdeOptions = options.Value;
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
                urn = WebUtility.UrlDecode(urn); // todo: inject or move to validator
                var validationResult = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                if (validationResult.InvalidResponseResult != null)
                    return validationResult.InvalidResponseResult;

                currentCorrelationId = validationResult.CurrentCorrelationId;
                var upstreamToken = validationResult.UpstreamToken;

                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                //var cdaScope = _configuration[ConfigurationKeys.CoreDataApiScope];
                //_logger.LogMethodFlow(currentCorrelationId, loggingName, $"Getting an access token as part of OBO for the following scope {cdaScope}");
                var onBehalfOfAccessToken = "not-implemented-yet"; // await _onBehalfOfTokenClient.GetAccessTokenAsync(validationResult.AccessTokenValue.ToJwtString(), cdaScope, currentCorrelationId);
                
                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Getting case details by Id {caseId}");
                caseDetails = await _caseDataService.GetCase(_caseDataArgFactory.CreateCaseArg(onBehalfOfAccessToken, upstreamToken, currentCorrelationId, urn, caseId));

                return caseDetails != null
                    ? new OkObjectResult(caseDetails)
                    : NotFoundErrorResponse($"No data found for case id '{caseId}'.", currentCorrelationId, loggingName);
            }
            catch (Exception exception)
            {
                return exception switch
                {
                    MsalException => InternalServerErrorResponse(exception, "An MSAL exception occurred.", currentCorrelationId, loggingName),
                    CaseDataServiceException => InternalServerErrorResponse(exception, "A case data api exception occurred.", currentCorrelationId, loggingName),
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

