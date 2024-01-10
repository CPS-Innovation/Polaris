using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using polaris_common.Factories.Contracts;

namespace polaris_common.Health
{
    public class AzureComputerVisionClientHealthCheck : IHealthCheck
    {
        private readonly ComputerVisionClient _computerVisionClient;

        public AzureComputerVisionClientHealthCheck(IComputerVisionClientFactory computerVisionClientFactory)
        {
            _computerVisionClient = computerVisionClientFactory.Create();
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _computerVisionClient.ListModelsAsync(cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch(Exception e)
            {
                return HealthCheckResult.Unhealthy(e.Message, e);
            }
        }
    }
}