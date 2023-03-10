using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PolarisGateway.CaseDataImplementations.Ddei.Clients;
using PolarisGateway.Domain.CaseData.Args;
using PolarisGateway.Factories.Contracts;

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
                var caseIds = (await _ddeiClient.ListCaseIdsAsync(urnArg)).ToList();

                return HealthCheckResult.Healthy($"{caseIds.Count} Case(s) for test URN");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(ex.Message, ex);
            }
        }
    }
}