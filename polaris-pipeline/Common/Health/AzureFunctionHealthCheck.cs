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
        private readonly string _service;

        public AzureFunctionHealthCheck(string service, IHttpClientFactory httpClientFactory)
        {
            _service = service;

            if (httpClientFactory is null)
                throw new ArgumentNullException(nameof(httpClientFactory));

            _httpClient = httpClientFactory.CreateClient(_service);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync("api/health");

                string content = await response.Content.ReadAsStringAsync();

                return (response.StatusCode == System.Net.HttpStatusCode.OK) ?
                    HealthCheckResult.Healthy(content) :
                    HealthCheckResult.Unhealthy(content);
            }
            catch (Exception e)
            {
                return HealthCheckResult.Unhealthy(e.Message, e);
            }
        }
    }
}