using Azure.Identity;
using Azure.Search.Documents;
using Azure.Storage.Blobs;
using Common.Clients;
using Common.Clients.Contracts;
using Common.Configuration;
using Common.Constants;
using Common.Factories;
using Common.Factories.Contracts;
using Common.Mappers;
using Common.Mappers.Contracts;
using Common.Services.SasGeneratorService;
using Common.Services.CaseSearchService;
using Common.Wrappers;
using Common.Wrappers.Contracts;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Common.Services.CaseSearchService.Contracts;

namespace Common.Services.Extensions
{
    public static class IServiceCollectionExtension
    {
        public static void AddBlobStorageWithDefaultAzureCredential(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddAzureClients(azureClientFactoryBuilder =>
            {
                var blobServiceUrl = configuration.GetValueFromConfig(ConfigKeys.SharedKeys.BlobServiceUrl);
                azureClientFactoryBuilder.AddBlobServiceClient(new Uri(blobServiceUrl))
                    .WithCredential(new DefaultAzureCredential());
            });

            services.AddTransient((Func<IServiceProvider, IPolarisStorageClient>)(serviceProvider =>
            {
                var logger = serviceProvider.GetService<ILogger<PolarisStorageClient>>();
                BlobServiceClient blobServiceClient = serviceProvider.GetRequiredService<BlobServiceClient>();
                string blobServiceContainerName = configuration.GetValueFromConfig(ConfigKeys.SharedKeys.BlobServiceContainerName);
                var blobStorageClient = new PolarisStorageClient(blobServiceClient, blobServiceContainerName, logger);

                return blobStorageClient;
            }));
        }

        public static void AddBlobStorageWithConnectionString(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddAzureClients(azureClientFactoryBuilder =>
            {
                string blobServiceConnectionString = configuration[ConfigKeys.SharedKeys.BlobServiceConnectionString];
                azureClientFactoryBuilder.AddBlobServiceClient(blobServiceConnectionString);
            });

            services.AddTransient((Func<IServiceProvider, IPolarisStorageClient>)(serviceProvider =>
            {
                var logger = serviceProvider.GetService<ILogger<PolarisStorageClient>>();
                BlobServiceClient blobServiceClient = serviceProvider.GetRequiredService<BlobServiceClient>();
                string blobServiceContainerName = configuration.GetValueFromConfig(ConfigKeys.SharedKeys.BlobServiceContainerName);
                return new PolarisStorageClient(blobServiceClient, blobServiceContainerName, logger);
            }));
        }

        public static void AddBlobSasGenerator(this IServiceCollection services)
        {
            services.AddTransient<IBlobSasBuilderWrapper, BlobSasBuilderWrapper>();
            services.AddTransient<IBlobSasBuilderFactory, BlobSasBuilderFactory>();
            services.AddTransient<IBlobSasBuilderWrapperFactory, BlobSasBuilderWrapperFactory>();
            services.AddTransient<ISasGeneratorService, SasGeneratorService.SasGeneratorService>();
        }

        public static void AddSearchClient(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddOptions<SearchClientOptions>().Configure<IConfiguration>((settings, _) =>
            {
                configuration.GetSection("searchClient").Bind(settings);
            });

            services.AddTransient<ICaseSearchClient, CaseSearchClient>();
            services.AddTransient<IAzureSearchClientFactory, AzureSearchClientFactory>();
            services.AddTransient<IStreamlinedSearchResultFactory, StreamlinedSearchResultFactory>();
            services.AddTransient<IStreamlinedSearchLineMapper, StreamlinedSearchLineMapper>();
            services.AddTransient<IStreamlinedSearchWordMapper, StreamlinedSearchWordMapper>();
        }
    }
}