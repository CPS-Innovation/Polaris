using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Common.Constants;
using Common.Domain.Exceptions;
using Common.Logging;
using coordinator.Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions
{
    public class CoordinatorStart
    {
        private readonly ILogger<CoordinatorStart> _logger;
        
        public CoordinatorStart(ILogger<CoordinatorStart> logger)
        {
            _logger = logger;
        }

        [FunctionName("CoordinatorStart")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cases/{caseUrn}/{caseId}")] HttpRequestMessage req,
                string caseUrn, string caseId, [DurableClient] IDurableOrchestrationClient orchestrationClient)
        {
            Guid currentCorrelationId = default;
            const string loggingName = $"{nameof(CoordinatorStart)} - {nameof(Run)}";

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
                    throw new BadRequestException("Invalid upstream token. A valid DDEI token must be received for this request.", nameof(req));
                var cmsAuthValues = cmsAuthValuesValues.First();
                if (string.IsNullOrWhiteSpace(cmsAuthValues))
                    throw new BadRequestException("Invalid upstream token. A valid DDEI token must be received for this request.", nameof(req));

                _logger.LogMethodEntry(currentCorrelationId, loggingName, req.RequestUri?.Query);

                if (string.IsNullOrWhiteSpace(caseUrn))
                    throw new BadRequestException("A case URN must be supplied.", caseUrn);

                if (!int.TryParse(caseId, out var caseIdNum))
                    throw new BadRequestException("Invalid case id. A 32-bit integer is required.", caseId);

                if (req.RequestUri == null)
                    throw new BadRequestException("Expected querystring value", nameof(req));

                var query = HttpUtility.ParseQueryString(req.RequestUri.Query);
                var force = query.Get("force");

                var forceRefresh = false;
                if (force != null && !bool.TryParse(force, out forceRefresh))
                {
                    throw new BadRequestException("Invalid query string. Force value must be a boolean.", force);
                }

                // Check if an instance with the specified ID already exists or an existing one stopped running(completed/failed/terminated/cancelled).
                _logger.LogMethodFlow(currentCorrelationId, loggingName, "Check if an instance with the specified ID already exists or an existing one stopped running(completed/failed/terminated/cancelled");
                var existingInstance = await orchestrationClient.GetStatusAsync(caseId);
                if (existingInstance == null || existingInstance.RuntimeStatus is OrchestrationRuntimeStatus.Completed
                        or OrchestrationRuntimeStatus.Failed or OrchestrationRuntimeStatus.Terminated or OrchestrationRuntimeStatus.Canceled)
                {
                    await orchestrationClient.StartNewAsync(
                        nameof(CoordinatorOrchestrator),
                        caseId,
                        new CoordinatorOrchestrationPayload(caseUrn, caseIdNum, forceRefresh, cmsAuthValues, currentCorrelationId));

                    _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Orchestrator StartUp Succeeded - Started {nameof(CoordinatorOrchestrator)} with instance id '{caseId}'");
                }
                else
                {
                    _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Orchestrator StartUp Succeeded - {nameof(CoordinatorOrchestrator)} with instance id '{caseId}' is already running");
                }

                return orchestrationClient.CreateCheckStatusResponse(req, caseId);
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
    }
}
