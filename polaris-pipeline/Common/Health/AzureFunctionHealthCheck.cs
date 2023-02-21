using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Common.Health
{
    // https://www.davidguida.net/azure-api-management-healthcheck/
    public class AzureFunctionHealthCheck : IHealthCheck
    {
        private readonly HttpClient _httpClient;

        public AzureFunctionHealthCheck(IHttpClientFactory httpClientFactory)
        {
            if (httpClientFactory is null)
                throw new ArgumentNullException(nameof(httpClientFactory));

            _httpClient = httpClientFactory.CreateClient(nameof(coordinator));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync("health");
            return (response.StatusCode == System.Net.HttpStatusCode.OK) ?
                HealthCheckResult.Healthy() :
                HealthCheckResult.Unhealthy();
        }
    }
}