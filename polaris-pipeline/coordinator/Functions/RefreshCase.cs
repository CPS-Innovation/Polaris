using Common.Configuration;
using Common.Extensions;
using Common.Domain.Exceptions;
using Common.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using coordinator.Durable.Providers;
using coordinator.Services.CleardownService;
using coordinator.Durable.Payloads;

namespace coordinator.Functions
{
    public class RefreshCase
    {
        private readonly ILogger<RefreshCase> _logger;
        private readonly IOrchestrationProvider _orchestrationProvider;
        private readonly ICleardownService _cleardownService;

        public RefreshCase(
            ILogger<RefreshCase> logger,
            IOrchestrationProvider orchestrationProvider,
            ICleardownService cleardownService)
        {
            _logger = logger;
            _orchestrationProvider = orchestrationProvider;
            _cleardownService = cleardownService;
        }

        [FunctionName(nameof(RefreshCase))]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.Locked)] // Refresh already running
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<HttpResponseMessage> Run
            (
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.Case)] HttpRequestMessage req,
                string caseUrn,
                int caseId,
                [DurableClient] IDurableOrchestrationClient orchestrationClient
            )
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();
                var cmsAuthValues = req.Headers.GetCmsAuthValues();

                var casePayload = new CaseOrchestrationPayload(caseUrn, caseId, cmsAuthValues, currentCorrelationId);

                return await _orchestrationProvider.RefreshCaseAsync(orchestrationClient, currentCorrelationId, caseId.ToString(), casePayload, req);
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

                _logger.LogMethodError(currentCorrelationId, nameof(RefreshCase), errorMessage, exception);

                return new HttpResponseMessage(httpStatusCode)
                {
                    Content = new StringContent(errorMessage, Encoding.UTF8, MediaTypeNames.Application.Json)
                };
            }
        }
    }
}