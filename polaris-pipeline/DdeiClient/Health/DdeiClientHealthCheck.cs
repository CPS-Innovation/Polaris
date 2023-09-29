using Common.Extensions;
using Common.Health;
using DdeiClient.Services.Contracts;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DDei.Health
{
    public class DdeiClientHealthCheck : AuthenticatedHealthCheck, IHealthCheck
    {
        private readonly IDdeiClient _ddeiClient;

        public DdeiClientHealthCheck(IDdeiClient ddeiClient)
        {
            _ddeiClient = ddeiClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _ddeiClient.GetStatus().WithTimeout(TimeSpan.FromSeconds(5));

                if (string.IsNullOrWhiteSpace(response))
                    return HealthCheckResult.Unhealthy("Null or empty response");

                return HealthCheckResult.Healthy($"DDEI Status : {response}");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(ex.Message, ex);
            }
        }
    }
}