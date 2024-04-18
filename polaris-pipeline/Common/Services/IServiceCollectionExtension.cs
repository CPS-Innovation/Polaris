using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using System;
using Azure.Identity;
using Azure.Storage.Blobs;
using Common.Services.BlobStorageService;
using Microsoft.Extensions.Logging;

namespace Common.Services
{
    public static class IServiceCollectionExtension
    {
        public const string BlobServiceContainerName = "BlobServiceContainerName";
        public const string BlobServiceUrl = nameof(BlobServiceUrl);

        public static void AddBlobStorageWithDefaultAzureCredential(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAzureClients(azureClientFactoryBuilder =>
            {
                var blobServiceUrl = GetValueFromConfig(configuration, BlobServiceUrl);
                var credentials = new DefaultAzureCredential();
                if (blobServiceUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    // our config has a url
                    azureClientFactoryBuilder.AddBlobServiceClient(new Uri(blobServiceUrl)).WithCredential(credentials);
                }
                else
                {
                    // our config is either a connection string "DefaultEndpointsProtocol=..." or, more probably,
                    //  UseDevelopmentStorage=true
                    azureClientFactoryBuilder.AddBlobServiceClient(blobServiceUrl).WithCredential(credentials);
                }

            });

            services.AddTransient((Func<IServiceProvider, IPolarisBlobStorageService>)(serviceProvider =>
            {
                var logger = serviceProvider.GetService<ILogger<PolarisBlobStorageService>>();
                var blobServiceClient = serviceProvider.GetRequiredService<BlobServiceClient>();
                var blobServiceContainerName = GetValueFromConfig(configuration, BlobServiceContainerName);
                return new PolarisBlobStorageService(blobServiceClient, blobServiceContainerName, logger);
            }));
        }

        public static string GetValueFromConfig(IConfiguration configuration, string secretName)
        {
            var secret = configuration[secretName];
            if (string.IsNullOrWhiteSpace(secret))
            {
                throw new Exception($"Secret cannot be null: {secretName}");
            }

            return secret;
        }
    }
}