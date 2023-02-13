using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using PolarisGateway.Domain.CaseData;
using PolarisGateway.Domain.Logging;
using PolarisGateway.Domain.Validators;
using PolarisGateway.Extensions;
using PolarisGateway.Services;
using PolarisGateway.Factories;
using System.Net;
using PolarisGateway.Domain.Exceptions;

namespace PolarisGateway.Functions.CaseData
{
    public class CaseDataApiCaseDocuments : BasePolarisFunction
    {
        private readonly ICaseDataService _caseDataService;
        private readonly ICaseDataArgFactory _caseDataArgFactory;
        private readonly ILogger<CaseDataApiCaseDocuments> _logger;

        public CaseDataApiCaseDocuments(ILogger<CaseDataApiCaseDocuments> logger, ICaseDataService caseDataService,
                                 IAuthorizationValidator tokenValidator, ICaseDataArgFactory caseDataArgFactory)
        : base(logger, tokenValidator)
        {
            _caseDataService = caseDataService;
            _caseDataArgFactory = caseDataArgFactory;
            _logger = logger;
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
                urn = WebUtility.UrlDecode(urn).GetUntilOrEmpty(); // todo: inject or move to validator
                var validationResult = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                if (validationResult.InvalidResponseResult != null)
                    return validationResult.InvalidResponseResult;

                currentCorrelationId = validationResult.CurrentCorrelationId;
                var cmsAuthValues = validationResult.CmsAuthValues;

                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                if (string.IsNullOrEmpty(urn))
                    return BadRequestErrorResponse("Urn is not supplied.", currentCorrelationId, loggingName);

                if (!caseId.HasValue)
                    return BadRequestErrorResponse("CaseId is not supplied.", currentCorrelationId, loggingName);

                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Getting case documents by Urn '{urn}' and CaseId '{caseId}'");
                documents = await _caseDataService.ListDocuments(_caseDataArgFactory.CreateCaseArg(cmsAuthValues, currentCorrelationId, urn, caseId.Value));

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
                    CaseDataServiceException => CmsAuthValuesErrorResponse(exception.Message, currentCorrelationId, loggingName),
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

