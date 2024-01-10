using Azure.Search.Documents;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using polaris_common.Factories.Contracts;

namespace polaris_common.Health
{
    public class AzureSearchClientHealthCheck : IHealthCheck
    {
        private readonly SearchClient _searchClient;

        public AzureSearchClientHealthCheck(IAzureSearchClientFactory searchClientFactory)
        {
            _searchClient = searchClientFactory.Create();
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _searchClient.GetDocumentCountAsync(cancellationToken);
                return HealthCheckResult.Healthy($"{(long)response} documents");
            }
            catch(Exception e)
            {
                return HealthCheckResult.Unhealthy(e.Message, e);
            }
        }
    }
}