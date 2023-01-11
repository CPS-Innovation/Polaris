using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using PolarisGateway.Clients.OnBehalfOfTokenClient;
using PolarisGateway.Domain.CaseData;
using PolarisGateway.Domain.Logging;
using PolarisGateway.Domain.Validators;
using PolarisGateway.Extensions;
using PolarisGateway.Services;
using PolarisGateway.Factories;
using System.Net;
using Microsoft.Extensions.Options;
using PolarisGateway.CaseDataImplementations.Ddei.Options;
using PolarisGateway.Domain.Exceptions;
using PolarisGateway.Helpers.Extension;

namespace PolarisGateway.Functions.CaseData
{
    public class CaseDataApiCaseDocuments : BasePolarisFunction
    {
        private readonly IOnBehalfOfTokenClient _onBehalfOfTokenClient;
        private readonly ICaseDataService _caseDataService;
        private readonly ICaseDataArgFactory _caseDataArgFactory;
        private readonly ILogger<CaseDataApiCaseDocuments> _logger;
        private readonly DdeiOptions _ddeiOptions;

        public CaseDataApiCaseDocuments(ILogger<CaseDataApiCaseDocuments> logger, IOnBehalfOfTokenClient onBehalfOfTokenClient, ICaseDataService caseDataService,
                                 IAuthorizationValidator tokenValidator, ICaseDataArgFactory caseDataArgFactory, IOptions<DdeiOptions> options)
        : base(logger, tokenValidator)
        {
            _onBehalfOfTokenClient = onBehalfOfTokenClient;
            _caseDataService = caseDataService;
            _caseDataArgFactory = caseDataArgFactory;
            _logger = logger;
            _ddeiOptions = options.Value;
        }

        [FunctionName("CaseDataApiCaseDocuments")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "urns/{urn}/cases/{caseId}/documents")] HttpRequest req,
            string urn,
            int? caseId)
        {
            Guid currentCorrelationId = default;
            const string loggingName = "CaseDataApiCaseDocuments - Run";
            IEnumerable<DocumentDetails> documents = null;

            try
            {
                urn = WebUtility.UrlDecode(urn); // todo: inject or move to validator
                var validationResult = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                if (validationResult.InvalidResponseResult != null)
                    return validationResult.InvalidResponseResult;

                currentCorrelationId = validationResult.CurrentCorrelationId;
                var upstreamToken = validationResult.UpstreamToken;

                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                if (string.IsNullOrEmpty(urn))
                    return BadRequestErrorResponse("Urn is not supplied.", currentCorrelationId, loggingName);

                if (!caseId.HasValue)
                    return BadRequestErrorResponse("CaseId is not supplied.", currentCorrelationId, loggingName);

                var ddeiScope = _ddeiOptions.DefaultScope;
                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Getting an access token as part of OBO for the following scope {ddeiScope}");
                //var onBehalfOfAccessToken = "not-implemented-yet"; // await _onBehalfOfTokenClient.GetAccessTokenAsync(validationResult.AccessTokenValue.ToJwtString(), cdaScope, currentCorrelationId);
                var onBehalfOfAccessToken = await _onBehalfOfTokenClient.GetAccessTokenAsync(validationResult.AccessTokenValue.ToJwtString(), ddeiScope, currentCorrelationId);

                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Getting case documents by Urn '{urn}' and CaseId '{caseId}'");
                documents = await _caseDataService.ListDocuments(_caseDataArgFactory.CreateCaseArg(onBehalfOfAccessToken, upstreamToken, currentCorrelationId, urn, caseId.Value));

                if (documents != null)
                {
                    return new OkObjectResult(documents);
                }

                return NotFoundErrorResponse($"No documents found for Urn '{urn}' and CaseId '{caseId}'.", currentCorrelationId, loggingName);
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
                _logger.LogMethodExit(currentCorrelationId, loggingName, documents.ToJson());
            }
        }
    }
}

