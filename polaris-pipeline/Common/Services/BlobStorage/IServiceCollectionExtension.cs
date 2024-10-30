using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using System;
using Azure.Identity;
using Azure.Storage.Blobs;
using Common.Wrappers;

namespace Common.Services.BlobStorage
{
    public static class IServiceCollectionExtension
    {
        private const string BlobServiceContainerNameDocuments = "BlobServiceContainerNameDocuments";
        private const string BlobServiceContainerNameThumbnails = "BlobServiceContainerNameThumbnails";
        public const string BlobServiceUrl = nameof(BlobServiceUrl);

        public static void AddBlobStorageWithDefaultAzureCredential(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAzureClients(azureClientFactoryBuilder =>
            {
                var blobServiceUrl = GetValueFromConfig(configuration, BlobServiceUrl);
                var credentials = new DefaultAzureCredential();
                if (blobServiceUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    // our config has an url
                    azureClientFactoryBuilder.AddBlobServiceClient(new Uri(blobServiceUrl)).WithCredential(credentials);
                }
                else
                {
                    // our config is either a connection string "DefaultEndpointsProtocol=..." or, more probably,
                    //  UseDevelopmentStorage=true
                    azureClientFactoryBuilder.AddBlobServiceClient(blobServiceUrl).WithCredential(credentials);
                }
            });

            services.AddTransient<Func<string, IBlobStorageService>>(serviceProvider => key =>
            {
                var blobServiceClient = serviceProvider.GetRequiredService<BlobServiceClient>();
                var jsonConvertWrapper = serviceProvider.GetRequiredService<IJsonConvertWrapper>();
              
                return key switch
                {
                    "Documents" => new BlobStorageService(blobServiceClient, GetValueFromConfig(configuration, BlobServiceContainerNameDocuments), jsonConvertWrapper),
                    "Thumbnails" => new BlobStorageService(blobServiceClient, GetValueFromConfig(configuration, BlobServiceContainerNameThumbnails), jsonConvertWrapper),
                    _ => throw new ArgumentException($"Unknown key: {key}")
                };
            });

            services.AddSingleton<IPolarisBlobStorageService, PolarisBlobStorageService>();
        }

        private static string GetValueFromConfig(IConfiguration configuration, string secretName)
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