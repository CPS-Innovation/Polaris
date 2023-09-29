using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace coordinator.Health
{
    public class OrchestrationProviderHealthCheck : IHealthCheck
    {
        public static DateTime? DeleteCalled { get; set; } = null;
        public static DateTime? DeleteBlobsByCaseSucceeded { get; set; } = null;
        public static DateTime? TerminatedOrchestrationInstancesSucceeded { get; set; } = null;
        public static DateTime? PurgedOrchestrationsAndDurableEntitiesSucceeded { get; set; } = null;

        private bool DeleteCompleted
        {
            get
            {
                return PurgedOrchestrationsAndDurableEntitiesSucceeded.HasValue;
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<HealthCheckResult> CheckHealthAsync(
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var result = $"DeleteCalled={DeleteCalled}, DeleteBlobsByCaseSucceeded={DeleteBlobsByCaseSucceeded}, TerminatedOrchestrationInstancesSucceeded (ListInstances)={TerminatedOrchestrationInstancesSucceeded}, PurgedOrchestrationsAndDurbleEntitiesSucceeded={PurgedOrchestrationsAndDurableEntitiesSucceeded}";

            if (!DeleteCalled.HasValue )
                return HealthCheckResult.Healthy("Case DELETE end point not yet called");

            if(DeleteCompleted)
                return HealthCheckResult.Healthy(result);

            return HealthCheckResult.Unhealthy(result);
        }
    }
}