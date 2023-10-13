//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.Azure.WebJobs.Extensions.DurableTask;
//using Microsoft.Extensions.Diagnostics.HealthChecks;

//namespace coordinator.Health
//{
//    public class IDurableOrchestrationClientHealthCheck : IHealthCheck
//    {
//        private readonly IDurableOrchestrationClient _durableOrchestrationClient;

//        // Can't DI - leave for now, need to delve into libraries for factory method
//        public IDurableOrchestrationClientHealthCheck(IDurableOrchestrationClient durableOrchestrationClient)
//        {
//            _durableOrchestrationClient = durableOrchestrationClient;
//        }

//#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
//        public async Task<HealthCheckResult> CheckHealthAsync(
//#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
//            HealthCheckContext context,
//            CancellationToken cancellationToken = default)
//        {
//            return HealthCheckResult.Healthy();
//        }
//    }
//}