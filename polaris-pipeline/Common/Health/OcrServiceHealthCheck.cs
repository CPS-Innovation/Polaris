using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Common.Services.OcrService;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Common.Health
{
    public class OcrServiceHealthCheck : IHealthCheck
    {
        private readonly IOcrService _ocrService;

        public OcrServiceHealthCheck(IOcrService ocrService)
        {
            _ocrService = ocrService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                bool mockService = false;
#if DEBUG
                mockService = _ocrService.GetType() == typeof(MockOcrService);
#endif

                var response = await _ocrService.GetOcrResultsAsync("not a blob", Guid.NewGuid());

                return HealthCheckResult.Healthy(mockService ? "(MOCKED SERVICE)" : $"Model Version ={response.ModelVersion}");
            }
            catch (Exception e)
            {
                return HealthCheckResult.Unhealthy($"{e.Message}", e);
            }
        }
    }
}