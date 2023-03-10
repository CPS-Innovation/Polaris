using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Services.DocumentExtractionService.Contracts;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Common.Health
{
    public class DdeiDocumentExtractionServiceHealthCheck : AuthenticatedHealthCheck, IHealthCheck
    {
        private readonly IDdeiDocumentExtractionService _ddeiDocumentExtractionService;

        static readonly string _testCaseUrn = "14XD1000422";
        static readonly string _testCaseId = "2148897";

        public DdeiDocumentExtractionServiceHealthCheck(IDdeiDocumentExtractionService ddeiDocumentExtractionService)
        {
            _ddeiDocumentExtractionService = ddeiDocumentExtractionService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _ddeiDocumentExtractionService.ListDocumentsAsync(_testCaseUrn, _testCaseId, CmsAuthValue, CorrelationId);

                return HealthCheckResult.Healthy($"{response.Length} document(s) for test case");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(ex.Message, ex);
            }
        }
    }
}