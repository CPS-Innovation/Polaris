using polaris_common.Services.BlobStorageService.Contracts;
using System.Reflection;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using polaris_common.Extensions;

namespace polaris_common.Health
{
    public class PolarisBlobStorageServiceHealthCheck : IHealthCheck
    {
        BlobServiceClient _blobServiceClient;
        IPolarisBlobStorageService _polarisBlobStorageService;

        public PolarisBlobStorageServiceHealthCheck(BlobServiceClient blobServiceClient, IPolarisBlobStorageService polarisBlobStorageService)
        {
            _blobServiceClient = blobServiceClient;
            _polarisBlobStorageService = polarisBlobStorageService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(PolarisBlobStorageServiceHealthCheck),"HealthCheck.pdf");

            try
            {
                var docName = "HealthCheck.pdf";
                var timeout = TimeSpan.FromSeconds(20);

                await _polarisBlobStorageService
                    .RemoveDocumentAsync(docName, Guid.NewGuid())
                    .WithTimeout(timeout);

                await _polarisBlobStorageService
                    .UploadDocumentAsync
                        (
                            stream,
                            docName,
                            "HealthCheck",
                            new ValueObjects.PolarisDocumentId(ValueObjects.PolarisDocumentType.CmsDocument, "HealthCheck"),
                            "1",
                            Guid.NewGuid()
                        )
                    .WithTimeout(timeout);

                return HealthCheckResult.Healthy($"{_blobServiceClient.Uri}documents/{docName}");
            }
            catch(Exception e)
            {
                return HealthCheckResult.Unhealthy(e.Message, e);
            }
        }
    }
}
