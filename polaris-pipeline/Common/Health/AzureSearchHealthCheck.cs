using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Search.Documents;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Common.Factories.Contracts;

namespace Common.Health
{
    public class AzureSearchHealthCheck : IHealthCheck
    {
        private readonly SearchClient _searchClient;

        public AzureSearchHealthCheck(ISearchClientFactory searchClientFactory)
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
                return HealthCheckResult.Healthy();
            }
            catch(Exception)
            {
                return HealthCheckResult.Unhealthy();
            }
        }
    }
}