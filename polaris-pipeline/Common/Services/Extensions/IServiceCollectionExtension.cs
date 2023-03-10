using Azure.Identity;
using Azure.Search.Documents;
using Azure.Storage.Blobs;
using Common.Clients;
using Common.Configuration;
using Common.Constants;
using Common.Factories;
using Common.Factories.Contracts;
using Common.Mappers;
using Common.Mappers.Contracts;
using Common.Services.SasGeneratorService;
using Common.Wrappers;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Common.Services.Extensions
{
    public static class IServiceCollectionExtension
    {
        public static void AddBlobStorage(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddAzureClients(azureClientFactoryBuilder =>
            {
                //string blobServiceConnectionString = configuration[ConfigKeys.SharedKeys.BlobServiceConnectionString];
                //azureClientFactoryBuilder.AddBlobServiceClient(blobServiceConnectionString);
                azureClientFactoryBuilder.AddBlobServiceClient(new Uri(configuration.GetValueFromConfig(ConfigKeys.SharedKeys.BlobServiceUrl)))
                    .WithCredential(new DefaultAzureCredential());
            });

            services.AddTransient((Func<IServiceProvider, IBlobStorageClient>)(serviceProvider =>
            {
                var logger = serviceProvider.GetService<ILogger<BlobStorageClient>>();
                BlobServiceClient blobServiceClient = serviceProvider.GetRequiredService<BlobServiceClient>();
                string blobServiceContainerName = configuration.GetValueFromConfig(ConfigKeys.SharedKeys.BlobServiceContainerName);
                return new BlobStorageClient(blobServiceClient, blobServiceContainerName, logger);
            }));

            /* services.AddTransient<IBlobStorageService>(serviceProvider =>
            {
                var loggingService = serviceProvider.GetService<ILogger<BlobStorageService.BlobStorageService>>();

                return new BlobStorageService.BlobStorageService(serviceProvider.GetRequiredService<BlobServiceClient>(),
                        configuration[ConfigKeys.SharedKeys.BlobServiceContainerName], loggingService);

            }); */
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

            services.AddTransient<ISearchIndexClient, SearchIndexClient>();
            services.AddTransient<ISearchClientFactory, SearchClientFactory>();
            services.AddTransient<IStreamlinedSearchResultFactory, StreamlinedSearchResultFactory>();
            services.AddTransient<IStreamlinedSearchLineMapper, StreamlinedSearchLineMapper>();
            services.AddTransient<IStreamlinedSearchWordMapper, StreamlinedSearchWordMapper>();
        }
    }
}