using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using System;
using System.Globalization;
using Azure.Identity;
using Azure.Storage.Blobs;
using Common.Configuration;
using Common.Wrappers;
using Common.Services.BlobStorage.Factories;

namespace Common.Services.BlobStorage
{
    public static class IServiceCollectionExtension
    {
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

            services.AddTransient<Func<string, IPolarisBlobStorageService>>(serviceProvider => key =>
            {
                var blobServiceClient = serviceProvider.GetRequiredService<BlobServiceClient>();
                var jsonConvertWrapper = serviceProvider.GetRequiredService<IJsonConvertWrapper>();

                return key.ToLower(CultureInfo.InvariantCulture) switch
                {
                    "documents" => new PolarisBlobStorageService(new BlobStorageService(blobServiceClient, GetValueFromConfig(configuration, StorageKeys.BlobServiceContainerNameDocuments), jsonConvertWrapper)),
                    "thumbnails" => new PolarisBlobStorageService(new BlobStorageService(blobServiceClient, GetValueFromConfig(configuration, StorageKeys.BlobServiceContainerNameThumbnails), jsonConvertWrapper)),
                    _ => throw new ArgumentException($"Unknown key: {key}")
                };
            });

            services.AddSingleton<IBlobTypeIdFactory, BlobTypeIdFactory>();
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