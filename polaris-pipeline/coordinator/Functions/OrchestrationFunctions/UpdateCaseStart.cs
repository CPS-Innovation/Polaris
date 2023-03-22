using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Common.Configuration;
using Common.Constants;
using Common.Domain.Exceptions;
using Common.Logging;
using coordinator.Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.OrchestrationFunctions
{
    public class UpdateCaseStart
    {
        private readonly ILogger<UpdateCaseStart> _logger;

        public UpdateCaseStart(ILogger<UpdateCaseStart> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(UpdateCaseStart))]
        public async Task<HttpResponseMessage> Run
            (
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", "delete", Route = RestApi.Case)] HttpRequestMessage req,
                string caseUrn,
                string caseId,
                [DurableClient] IDurableOrchestrationClient orchestrationClient
            )
        {
            Guid currentCorrelationId = default;
            const string loggingName = $"{nameof(UpdateCaseStart)} - {nameof(Run)}";

            try
            {
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

                _logger.LogMethodEntry(currentCorrelationId, loggingName, req.RequestUri?.Query);

                if (string.IsNullOrWhiteSpace(caseUrn))
                    throw new BadRequestException("A case URN must be supplied.", caseUrn);

                if (!int.TryParse(caseId, out var caseIdNum))
                    throw new BadRequestException("Invalid case id. A 32-bit integer is required.", caseId);

                if (req.RequestUri == null)
                    throw new BadRequestException("Expected querystring value", nameof(req));

                switch (req.Method.Method)
                {
                    case "POST":
                        var existingInstance = await orchestrationClient.GetStatusAsync(caseId);
                        bool isRunning = IsRunning(existingInstance);
                        if (!isRunning)
                            await orchestrationClient.StartNewAsync
                            (
                                nameof(RefreshCaseOrchestrator),
                                caseId,
                                new CaseOrchestrationPayload(caseUrn, caseIdNum, cmsAuthValues, currentCorrelationId)
                            );
                        _logger.LogMethodFlow(currentCorrelationId, loggingName, $"{nameof(UpdateCaseStart)} Succeeded - Started {nameof(RefreshCaseOrchestrator)} with instance id '{caseId}'");

                        return orchestrationClient.CreateCheckStatusResponse(req, caseId);

                    case "DELETE":
                        var status = await orchestrationClient.GetStatusAsync(caseId);
                        await orchestrationClient.StartNewAsync
                            (
                                nameof(DeleteCaseOrchestrator),
                                caseId,
                                new CaseOrchestrationPayload(caseUrn, caseIdNum, cmsAuthValues, currentCorrelationId)
                            ); 
                        await orchestrationClient.TerminateAsync(status.InstanceId, $"{loggingName} - terminated via DELETE");

                        return new HttpResponseMessage(HttpStatusCode.Accepted);

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
            finally
            {
                _logger.LogMethodExit(currentCorrelationId, loggingName, "n/a");
            }
        }

        private static bool IsRunning(DurableOrchestrationStatus existingInstance)
        {
            bool notRunning = existingInstance == null ||
                    existingInstance.RuntimeStatus
                        is OrchestrationRuntimeStatus.Completed
                        or OrchestrationRuntimeStatus.Failed
                        or OrchestrationRuntimeStatus.Terminated
                        or OrchestrationRuntimeStatus.Canceled;

            return !notRunning;
        }
    }
}
