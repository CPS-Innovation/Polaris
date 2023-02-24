using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Services.SearchIndexService;
using Common.Services.SearchIndexService.Contracts;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Common.Health
{
    public class SearchIndexServiceHealthCheck : IHealthCheck
    {
        private readonly ISearchIndexService _searchIndexService;

        public SearchIndexServiceHealthCheck(ISearchIndexService searchIndexService)
        {
            _searchIndexService = searchIndexService;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<HealthCheckResult> CheckHealthAsync(
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                bool mockService = false;
#if DEBUG
                mockService = _searchIndexService.GetType() == typeof(MockSearchIndexService);
#endif

                return HealthCheckResult.Healthy(mockService ? "(MOCKED SERVICE)" : $"Instantiated (not called)");
            }
            catch (Exception e)
            {
                return HealthCheckResult.Unhealthy($"{e.Message}", e);
            }
        }
    }
}