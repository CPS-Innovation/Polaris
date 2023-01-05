using Common.Wrappers;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using text_extractor.Handlers;
using text_extractor.Services.OcrService;
using text_extractor.Services.SasGeneratorService;
using text_extractor.Factories;
using text_extractor.Wrappers;
using Azure.Identity;
using System;
using System.Diagnostics.CodeAnalysis;
using Common.Constants;
using Common.Domain.QueueItems;
using Common.Domain.Requests;
using Common.Exceptions.Contracts;
using Common.Factories;
using Common.Factories.Contracts;
using Common.Handlers;
using Common.Services.SearchIndexService;
using Common.Services.SearchIndexService.Contracts;
using Common.Services.StorageQueueService;
using Common.Services.StorageQueueService.Contracts;

[assembly: FunctionsStartup(typeof(text_extractor.Startup))]
namespace text_extractor
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
            builder.Services.AddSingleton<IOcrService, OcrService>();
            builder.Services.AddSingleton<ISearchIndexService, SearchIndexService>();
            builder.Services.AddTransient<ISasGeneratorService, SasGeneratorService>();
            builder.Services.AddAzureClients(azureClientFactoryBuilder =>
            {
                azureClientFactoryBuilder.AddBlobServiceClient(new Uri($"{configuration[ConfigKeys.SharedKeys.BlobServiceUrl]}"))
                    .WithCredential(new DefaultAzureCredential());
            });
            builder.Services.AddTransient<IExceptionHandler, ExceptionHandler>();
            builder.Services.AddTransient<IAuthorizationValidator, AuthorizationValidator>();
            builder.Services.AddTransient<IValidatorWrapper<ExtractTextRequest>, ValidatorWrapper<ExtractTextRequest>>();
            builder.Services.AddTransient<IValidatorWrapper<UpdateSearchIndexByBlobNameQueueItem>, ValidatorWrapper<UpdateSearchIndexByBlobNameQueueItem>>();
            builder.Services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            builder.Services.AddTransient<IBlobSasBuilderWrapper, BlobSasBuilderWrapper>();
            builder.Services.AddTransient<IBlobSasBuilderFactory, BlobSasBuilderFactory>();
            builder.Services.AddTransient<IBlobSasBuilderWrapperFactory, BlobSasBuilderWrapperFactory>();
            builder.Services.AddTransient<ISearchLineFactory, SearchLineFactory>();
            builder.Services.AddTransient<ISearchClientFactory, SearchClientFactory>();
            builder.Services.AddTransient<IComputerVisionClientFactory, ComputerVisionClientFactory>();
            builder.Services.AddTransient<ISearchIndexingBufferedSenderFactory, SearchIndexingBufferedSenderFactory>();
            builder.Services.AddTransient<IStorageQueueHelper, StorageQueueHelper>();
            builder.Services.AddTransient<IStorageQueueService>(provider =>
            {
                var helper = provider.GetService<IStorageQueueHelper>();
                return new StorageQueueService(configuration[ConfigKeys.SharedKeys.DocumentEvaluatorQueueUrl], helper);
            });
        }
    }
}