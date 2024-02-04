using Common.Configuration;
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
using coordinator.Providers;
using Common.Extensions;

namespace coordinator.Functions
{
    public class DeleteCase
    {
        private readonly ILogger<DeleteCase> _logger;
        private readonly IOrchestrationProvider _orchestrationProvider;

        public DeleteCase(
            ILogger<DeleteCase> logger,
            IOrchestrationProvider orchestrationProvider)
        {
            _logger = logger;
            _orchestrationProvider = orchestrationProvider;
        }

        [FunctionName(nameof(DeleteCase))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
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

                await _orchestrationProvider.DeleteCaseAsync(orchestrationClient,
                                                             currentCorrelationId,
                                                             caseUrn,
                                                             caseId,
                                                             checkForBlobProtection: false,
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