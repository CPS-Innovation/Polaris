using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Search.Documents;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using text_extractor.Factories.Contracts;

namespace text_extractor.Health
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
            catch (Exception e)
            {
                return HealthCheckResult.Unhealthy(e.Message, e);
            }
        }
    }
}