using Common.Configuration;
using Common.Constants;
using Common.Domain.Exceptions;
using Common.Logging;
using coordinator.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using coordinator.Providers;
using coordinator.Services.CleardownService;

namespace coordinator.Functions.Orchestration.Client.Case
{
    public class CaseClient
    {
        private readonly ILogger<CaseClient> _logger;
        private readonly IOrchestrationProvider _orchestrationProvider;
        private readonly ICleardownService _cleardownService;

        public CaseClient(
            ILogger<CaseClient> logger,
            IOrchestrationProvider orchestrationProvider,
            ICleardownService cleardownService)
        {
            _logger = logger;
            _orchestrationProvider = orchestrationProvider;
            _cleardownService = cleardownService;
        }

        [FunctionName(nameof(CaseClient))]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.Locked)] // Refresh already running
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<HttpResponseMessage> Run
            (
                [HttpTrigger(AuthorizationLevel.Anonymous, "put", "delete", "post", Route = RestApi.Case)] HttpRequestMessage req,
                string caseUrn,
                string caseId,
                [DurableClient] IDurableOrchestrationClient orchestrationClient
            )
        {
            Guid currentCorrelationId = default;
            const string loggingName = $"{nameof(CaseClient)} - {nameof(Run)}";

            try
            {
                #region Validate-Inputs

                req.Headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIdValues);
                if (correlationIdValues == null)
                    throw new BadRequestException("Invalid correlationId. A valid GUID is required.", nameof(req));

                var correlationId = correlationIdValues.FirstOrDefault();
                if (!Guid.TryParse(correlationId, out currentCorrelationId))
                    if (currentCorrelationId == Guid.Empty)
                        throw new BadRequestException("Invalid correlationId. A valid GUID is required.", correlationId);

                req.Headers.TryGetValues(HttpHeaderKeys.CmsAuthValues, out var cmsAuthValuesValues);
                if (cmsAuthValuesValues == null)
                    throw new BadRequestException("Invalid Cms Auth token. A valid Cms Auth token must be received for this request.", nameof(req));
                var cmsAuthValues = cmsAuthValuesValues.First();
                if (string.IsNullOrWhiteSpace(cmsAuthValues))
                    throw new BadRequestException("Invalid Cms Auth token. A valid Cms Auth token must be received for this request.", nameof(req));

                if (string.IsNullOrWhiteSpace(caseUrn))
                    throw new BadRequestException("A case URN must be supplied.", caseUrn);

                if (!int.TryParse(caseId, out var caseIdNum))
                    throw new BadRequestException("Invalid case id. A 32-bit integer is required.", caseId);

                if (req.RequestUri == null)
                    throw new BadRequestException("Expected querystring value", nameof(req));

                var baseUrl = req.RequestUri.GetLeftPart(UriPartial.Authority);
                var extensionCode = HttpUtility.ParseQueryString(req.RequestUri.Query).Get("code");

                #endregion

                var casePayload = new CaseOrchestrationPayload(caseUrn, caseIdNum, baseUrl, extensionCode, cmsAuthValues, currentCorrelationId);

                switch (req.Method.Method)
                {
                    case "POST":
                        return await _orchestrationProvider.RefreshCaseAsync(orchestrationClient, currentCorrelationId, caseId, casePayload, req);

                    case "DELETE":
                        await _cleardownService.DeleteCaseAsync(orchestrationClient,
                                                                     caseUrn,
                                                                     caseIdNum,
                                                                     currentCorrelationId,
                                                                     waitForIndexToSettle: true);
                        return new HttpResponseMessage(HttpStatusCode.OK);

                    default:
                        throw new BadRequestException("Unexpected HTTP Verb", req.Method.Method);
                }
            }
            catch (Exception exception)
            {
                var rootCauseMessage = "An unhandled exception occurred";
                var httpStatusCode = HttpStatusCode.InternalServerError;

                if (exception is UnauthorizedException)
                {
                    rootCauseMessage = "Unauthorized";
                    httpStatusCode = HttpStatusCode.Unauthorized;
                }
                else if (exception is BadRequestException)
                {
                    rootCauseMessage = "Invalid request";
                    httpStatusCode = HttpStatusCode.BadRequest;
                }

                var errorMessage = $"{rootCauseMessage}. {exception.Message}.  Base exception message: {exception.GetBaseException().Message}";

                _logger.LogMethodError(currentCorrelationId, loggingName, errorMessage, exception);

                return new HttpResponseMessage(httpStatusCode)
                {
                    Content = new StringContent(errorMessage, Encoding.UTF8, MediaTypeNames.Application.Json)
                };
            }
        }
    }
}