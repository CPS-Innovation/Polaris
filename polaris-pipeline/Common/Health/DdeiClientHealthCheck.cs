//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Diagnostics.HealthChecks;

//namespace Common.Health
//{
//    public class DdeiClientHealthCheck : AuthenticatedHealthCheck, IHealthCheck
//    {
//        private readonly IDdeiClient _ddeiClient;

//        static readonly string _testCaseUrn = "14XD1000422";
//        static readonly string _testCaseId = "2148897";

//        public DdeiClientHealthCheck(IDdeiClient ddeiClient)
//        {
//            _ddeiClient = ddeiClient;
//        }

//        public async Task<HealthCheckResult> CheckHealthAsync(
//            HealthCheckContext context,
//            CancellationToken cancellationToken = default)
//        {
//            try
//            {
//                //var response = await _ddeiDocumentExtractionService.ListDocumentsAsync(_testCaseUrn, _testCaseId, CmsAuthValue, CorrelationId);

//                return HealthCheckResult.Healthy($"");
//            }
//            catch (Exception ex)
//            {
//                return HealthCheckResult.Unhealthy(ex.Message, ex);
//            }
//        }
//    }
//}