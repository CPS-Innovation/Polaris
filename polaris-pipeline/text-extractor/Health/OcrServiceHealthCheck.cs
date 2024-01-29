using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using text_extractor.Services.OcrService;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace text_extractor.Health
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