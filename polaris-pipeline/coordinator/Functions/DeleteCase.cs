using Common.Configuration;
using Common.Constants;
using Common.Domain.Exceptions;
using Common.Logging;
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
using coordinator.Durable.Providers;
using coordinator.Services.CleardownService;
using Common.Extensions;

namespace coordinator.Functions
{
    public class DeleteCase
    {
        private readonly ILogger<DeleteCase> _logger;
        private readonly IOrchestrationProvider _orchestrationProvider;
        private readonly ICleardownService _cleardownService;

        public DeleteCase(
            ILogger<DeleteCase> logger,
            IOrchestrationProvider orchestrationProvider,
            ICleardownService cleardownService)
        {
            _logger = logger;
            _orchestrationProvider = orchestrationProvider;
            _cleardownService = cleardownService;
        }

        [FunctionName(nameof(DeleteCase))]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.Locked)] // Refresh already running
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<HttpResponseMessage> Run
            (
                [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = RestApi.Case)] HttpRequestMessage req,
                string caseUrn,
                int caseId,
                [DurableClient] IDurableOrchestrationClient orchestrationClient
            )
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();

                await _cleardownService.DeleteCaseAsync(orchestrationClient,
                     caseUrn,
                     caseId,
                     currentCorrelationId,
                     waitForIndexToSettle: true);

                return new HttpResponseMessage(HttpStatusCode.OK);

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

                _logger.LogMethodError(currentCorrelationId, nameof(DeleteCase), errorMessage, exception);

                return new HttpResponseMessage(httpStatusCode)
                {
                    Content = new StringContent(errorMessage, Encoding.UTF8, MediaTypeNames.Application.Json)
                };
            }
        }
    }
}