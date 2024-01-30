using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using text_extractor.Factories.Contracts;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;

namespace text_extractor.Health
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