using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Common.Configuration;
using Common.Constants;
using System;
using Azure.Identity;
using Azure.Storage.Blobs;
using Common.Services.BlobStorageService.Contracts;
using Microsoft.Extensions.Logging;
using Common.Services.BlobStorageService;

namespace Common.Services.Extensions
{
    public static class IServiceCollectionExtension
    {
        public static void AddBlobStorageWithDefaultAzureCredential(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAzureClients(azureClientFactoryBuilder =>
            {
                var blobServiceUrl = configuration.GetValueFromConfig(ConfigKeys.SharedKeys.BlobServiceUrl);
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
                var blobServiceContainerName = configuration.GetValueFromConfig(ConfigKeys.SharedKeys.BlobServiceContainerName);
                return new PolarisBlobStorageService(blobServiceClient, blobServiceContainerName, logger);
            }));
        }
    }
}