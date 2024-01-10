using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using polaris_common.Services.OcrService;

namespace polaris_common.Health
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

                var response = await _ocrService.GetOcrResultsAsync(new MemoryStream(), Guid.NewGuid());
                string json = JsonSerializer.Serialize(response);

                return HealthCheckResult.Healthy(mockService ? "(MOCKED SERVICE)" : $"Model Version={response.ModelVersion}");
            }
            catch (Exception e)
            {
                return HealthCheckResult.Unhealthy($"{e.Message}", e);
            }
        }
    }
}