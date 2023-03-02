using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Common.Health
{
    public class AzureBlobServiceClientHealthCheck : IHealthCheck
    {
        private readonly BlobServiceClient _blobServiceClient;

        public AzureBlobServiceClientHealthCheck(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var accountInfo = (AccountInfo)(await _blobServiceClient.GetAccountInfoAsync(cancellationToken));

                string host = _blobServiceClient.Uri.Host;
                var isAzuriteEmulator = (host == "localhost") || (host == "127.0.0.1");

                return HealthCheckResult.Healthy($"{(isAzuriteEmulator ? "Azurite Emulator" : "Azure Blob Storage")} : {accountInfo.AccountKind}, {accountInfo.SkuName}"); 
            }
            catch(Exception e)
            {
                return HealthCheckResult.Unhealthy(e.Message, e);
            }
        }
    }
}