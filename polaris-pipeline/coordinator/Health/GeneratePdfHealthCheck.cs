using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace coordinator.Health
{
    public class GeneratePdfHealthCheck : IHealthCheck
    {
        public static DateTime? RunCalled { get; set; } = null;
        public static DateTime? InputsValidated { get; set; } = null;
        public static DateTime? CmsDocumentRetrieved { get; set; } = null;
        public static DateTime? ConvertedToPdf { get; set; } = null;
        public static DateTime? UploadedToBlobStorage { get; set; } = null;
        public static string LastException { get; set; } = null;

        private bool Completed
        {
            get
            {
                return UploadedToBlobStorage.HasValue;
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<HealthCheckResult> CheckHealthAsync(
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var result = $"RunCalled={RunCalled}, InputsValidated={InputsValidated}, CmsDocumentRetrieved={CmsDocumentRetrieved}, ConvertedToPdf={ConvertedToPdf}, UploadedToBlobStorage={UploadedToBlobStorage}, LastException={LastException}";

            if (!RunCalled.HasValue)
                return HealthCheckResult.Healthy($"PDF Generation not yet called : {result}");

            if(!Completed)
                return HealthCheckResult.Unhealthy(result);

            return HealthCheckResult.Healthy(result);
        }
    }
}