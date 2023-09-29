using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Common.Extensions;

namespace Common.Health
{
    public class AzureBlobServiceClientHealthCheck : IHealthCheck
    {
        private readonly BlobServiceClient _blobServiceClient;

        public AzureBlobServiceClientHealthCheck(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<HealthCheckResult> CheckHealthAsync(
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var container 
                    = await Task.Run(() => _blobServiceClient.GetBlobContainerClient("documents"))
                                .WithTimeout(TimeSpan.FromSeconds(5)); 

                if(!container.Uri.PathAndQuery.EndsWith("documents"))
                    return HealthCheckResult.Unhealthy($"Container Uri is not correct: {container.Uri}");

                return HealthCheckResult.Healthy(container.Uri.ToString()); 
            }
            catch(Exception e)
            {
                return HealthCheckResult.Unhealthy(e.Message, e);
            }
        }
    }
}