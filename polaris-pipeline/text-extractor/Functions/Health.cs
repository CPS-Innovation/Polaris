using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Common.Constants;
using Common.Domain.Exceptions;
using System;
using Microsoft.Extensions.Logging;
using Common.Logging;
using Common.Health;
using Common.Domain.Extensions;
using Common.Configuration;

public class Health
{
    private readonly HealthCheckService _healthService;
    private readonly ILogger<Health> _log;
    private readonly string loggingName = $"{nameof(Health)}.{nameof(Healthcheck)} - Run";

    public Health(HealthCheckService healthService, ILogger<Health> logger)
    {
        _healthService = healthService ?? throw new ArgumentNullException(nameof(healthService));
        _log = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [FunctionName(nameof(Health))]
    public async Task<IActionResult> Healthcheck(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = RestApi.Health)]
        HttpRequest request)
    {
        try
        {
            request.Headers.TryGetValue(HttpHeaderKeys.CorrelationId, out var correlationIdValues);
            if (correlationIdValues.Count == 0)
                throw new BadRequestException("Invalid correlationId. A valid GUID is required.", nameof(request));

            var correlationId = correlationIdValues.First();
            Guid currentCorrelationId = default;
            if (!Guid.TryParse(correlationId, out currentCorrelationId) || currentCorrelationId == Guid.Empty)
                throw new BadRequestException("Invalid correlationId. A valid GUID is required.", correlationId);

            request.Headers.TryGetValue(HttpHeaderKeys.CmsAuthValues, out var cmsAuthValuesValues);
            if (cmsAuthValuesValues.Count == 0)
                throw new BadRequestException("Invalid Cms Auth token. A valid Cms Auth token must be received for this request.", nameof(request));
            var cmsAuthValues = cmsAuthValuesValues.First();
            if (string.IsNullOrWhiteSpace(cmsAuthValues))
                throw new BadRequestException("Invalid Cms Auth token. A valid Cms Auth token must be received for this request.", nameof(request));

            _log.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

            // Shim in the auth into any health checks needing auth
            AuthenticatedHealthCheck.SetAuthValues(cmsAuthValues, currentCorrelationId);

            var healthResult = await _healthService.CheckHealthAsync();
            var status = (healthResult.Status == HealthStatus.Healthy) ? 200 : 500;

            return new JsonResult(healthResult.ToResult())
            {
                StatusCode = status
            };
        }
        catch (Exception ex)
        {
            return new JsonResult(new
            {
                status = ex.ToString(),
            })
            {
                StatusCode = 500
            };
        }
    }
}
