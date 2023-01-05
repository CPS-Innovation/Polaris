using System;
using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using Azure.Storage.Blobs;
using Common.Constants;
using Common.Domain.QueueItems;
using Common.Domain.Requests;
using Common.Exceptions.Contracts;
using Common.Factories;
using Common.Factories.Contracts;
using Common.Handlers;
using Common.Services.BlobStorageService;
using Common.Services.BlobStorageService.Contracts;
using Common.Services.StorageQueueService;
using Common.Services.StorageQueueService.Contracts;
using Common.Wrappers;
using document_evaluator.Domain.Handlers;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(document_evaluator.Startup))]
namespace document_evaluator
{
    [ExcludeFromCodeCoverage]
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .Build();
            
            builder.Services.AddSingleton<IConfiguration>(configuration);
            
            builder.Services.AddTransient<IValidatorWrapper<ProcessDocumentsToRemoveRequest>, ValidatorWrapper<ProcessDocumentsToRemoveRequest>>();
            builder.Services.AddTransient<IValidatorWrapper<UpdateSearchIndexByBlobNameQueueItem>, ValidatorWrapper<UpdateSearchIndexByBlobNameQueueItem>>();
            builder.Services.AddTransient<IValidatorWrapper<UpdateSearchIndexByVersionQueueItem>, ValidatorWrapper<UpdateSearchIndexByVersionQueueItem>>();
            builder.Services.AddTransient<IValidatorWrapper<UpdateBlobStorageQueueItem>, ValidatorWrapper<UpdateBlobStorageQueueItem>>();
            builder.Services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            builder.Services.AddTransient<IAuthorizationValidator, AuthorizationValidator>();
            builder.Services.AddTransient<IExceptionHandler, ExceptionHandler>();
            
            builder.Services.AddAzureClients(azureClientFactoryBuilder =>
            {
                azureClientFactoryBuilder.AddBlobServiceClient(new Uri(configuration[ConfigKeys.SharedKeys.BlobServiceUrl]))
                    .WithCredential(new DefaultAzureCredential());
            });
            builder.Services.AddTransient<IBlobStorageService>(serviceProvider =>
            {
                var loggingService = serviceProvider.GetService<ILogger<BlobStorageService>>();
                
                return new BlobStorageService(serviceProvider.GetRequiredService<BlobServiceClient>(),
                    configuration[ConfigKeys.SharedKeys.BlobServiceContainerName], loggingService);
            });

            builder.Services.AddTransient<IHttpRequestFactory, HttpRequestFactory>();

            builder.Services.AddTransient<ISearchClientFactory, SearchClientFactory>();
            builder.Services.AddTransient<IStorageQueueHelper, StorageQueueHelper>();
            builder.Services.AddTransient<IStorageQueueService>(provider =>
            {
                var helper = provider.GetService<IStorageQueueHelper>();
                return new StorageQueueService(configuration[ConfigKeys.SharedKeys.DocumentEvaluatorQueueUrl], helper);
            });
        }
    }
}
