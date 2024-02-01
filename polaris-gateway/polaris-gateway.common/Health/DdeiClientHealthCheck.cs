using Ddei.Factories.Contracts;
using DdeiClient.Services.Contracts;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Common.Health
{
    public class DdeiClientHealthCheck : AuthenticatedHealthCheck, IHealthCheck
    {
        private readonly ICaseDataArgFactory _caseDataArgFactory;
        private readonly IDdeiClient _ddeiClient;

        static readonly string _testCaseUrn = "14XD1000422";

        public DdeiClientHealthCheck(IDdeiClient ddeiClient, ICaseDataArgFactory caseDataArgFactory)
        {
            _ddeiClient = ddeiClient;
            _caseDataArgFactory = caseDataArgFactory;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var urnArg = _caseDataArgFactory.CreateUrnArg(CmsAuthValue, CorrelationId, _testCaseUrn);
                var caseIds = (await _ddeiClient.ListCasesAsync(urnArg)).ToList();

                return HealthCheckResult.Healthy($"{caseIds.Count} Case(s) for test URN");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(ex.Message, ex);
            }
        }
    }
}