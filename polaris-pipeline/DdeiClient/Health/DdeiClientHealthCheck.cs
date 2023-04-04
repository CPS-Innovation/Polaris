using Common.Health;
using DdeiClient.Services.Contracts;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DDei.Health
{
    public class DdeiClientHealthCheck : AuthenticatedHealthCheck, IHealthCheck
    {
        private readonly IDdeiClient _ddeiClient;

        static readonly string _testCaseUrn = "14XD1000422";
        static readonly string _testCaseId = "2148897";

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
                var response = await _ddeiClient.ListDocumentsAsync(_testCaseUrn, _testCaseId, CmsAuthValue, CorrelationId);

                return HealthCheckResult.Healthy($"{response.Length} document(s) for test case");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(ex.Message, ex);
            }
        }
    }
}