using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Common.Configuration;
using Common.Constants;
using Common.Domain.Extensions;
using Common.Factories;
using Common.Factories.Contracts;
using Common.Mappers;
using Common.Mappers.Contracts;
using Common.Services.Extensions;
using Common.Wrappers;
using coordinator;
using coordinator.Factories;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Common.Health;
using Common.Wrappers.Contracts;
using Common.Clients.Contracts;
using Common.Clients;
using FluentValidation;
using Common.Domain.Validators;
using System.IO;
using Common.Dto.Request;
using Ddei.Services.Extensions;
using Azure.Storage.Blobs;
using Common.Services.BlobStorageService.Contracts;
using Common.Services.BlobStorageService;
using Microsoft.Extensions.Logging;
using coordinator.Functions;

[assembly: FunctionsStartup(typeof(Startup))]
namespace coordinator
{
    [ExcludeFromCodeCoverage]
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
#if DEBUG
                .SetBasePath(Directory.GetCurrentDirectory())
#endif
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .Build();

            builder.Services.AddSingleton<IConfiguration>(configuration);
            builder.Services.AddTransient<IDefaultAzureCredentialFactory, DefaultAzureCredentialFactory>();
            builder.Services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            builder.Services.AddSingleton<IGeneratePdfHttpRequestFactory, GeneratePdfHttpRequestFactory>();
            builder.Services.AddSingleton<ITextExtractorHttpRequestFactory, TextExtractorHttpRequestFactory>();
            builder.Services.AddTransient<IPipelineClientRequestFactory, PipelineClientRequestFactory>();

            builder.Services.AddTransient<IPolarisBlobStorageService>(serviceProvider =>
            {
                var loggingService = serviceProvider.GetService<ILogger<PolarisBlobStorageService>>();

                return new PolarisBlobStorageService(serviceProvider.GetRequiredService<BlobServiceClient>(),
                        configuration[ConfigKeys.SharedKeys.BlobServiceContainerName], loggingService);
            });

            // Redact PDF
            builder.Services.AddHttpClient<IRedactionClient, RedactionClient>(client =>
            {
                client.BaseAddress = new Uri(configuration.GetValueFromConfig(PipelineSettings.PipelineRedactPdfBaseUrl));
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });
            builder.Services.AddTransient<IRedactPdfRequestMapper, RedactPdfRequestMapper>();
            builder.Services.AddScoped<IValidator<RedactPdfRequestDto>, RedactPdfRequestValidator>();

            builder.Services.AddBlobStorageWithDefaultAzureCredential(configuration);
            builder.Services.AddBlobSasGenerator();
            builder.Services.AddSearchClient(configuration);
            builder.Services.AddDdeiClient(configuration);
            builder.Services.AddPdfGenerator();

            var pipelineRedactPdfBaseUrl = new Uri(configuration.GetValueFromConfig(PipelineSettings.PipelineRedactPdfBaseUrl));
            builder.Services.AddHttpClient(nameof(GeneratePdf), client =>
            {
                client.BaseAddress = pipelineRedactPdfBaseUrl;
            });

            BuildHealthChecks(builder);
        }

        /// <summary>
        /// see https://www.davidguida.net/azure-api-management-healthcheck/ for pattern
        /// Microsoft.Extensions.Diagnostics.HealthChecks Nuget downgraded to lower release to get package to work
        /// </summary>
        /// <param name="builder"></param>
        private static void BuildHealthChecks(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();

            var pdfGeneratorFunction = "pdfGeneratorFunction";
            builder.Services.AddHttpClient(pdfGeneratorFunction, client =>
            {
                string url = Environment.GetEnvironmentVariable("PdfGeneratorUrl");
                client.BaseAddress = new Uri(url.GetBaseUrl());
                client.DefaultRequestHeaders.Add("Cms-Auth-Values", AuthenticatedHealthCheck.CmsAuthValue);
                client.DefaultRequestHeaders.Add("Correlation-Id", AuthenticatedHealthCheck.CorrelationId.ToString());
            });

            var textExtractorFunction = "textExtractorFunction";
            builder.Services.AddHttpClient(textExtractorFunction, client =>
            {
                string url = Environment.GetEnvironmentVariable("TextExtractorUrl");
                client.BaseAddress = new Uri(url.GetBaseUrl());
                client.DefaultRequestHeaders.Add("Cms-Auth-Values", AuthenticatedHealthCheck.CmsAuthValue);
                client.DefaultRequestHeaders.Add("Correlation-Id", AuthenticatedHealthCheck.CorrelationId.ToString());
            });

            builder.Services.AddHealthChecks()
                .AddCheck<AzureBlobServiceClientHealthCheck>("Azure Blob Service Client")
                .AddCheck<AzureSearchClientHealthCheck>("Azure Search Client")
                .AddTypeActivatedCheck<AzureFunctionHealthCheck>("PDF Generator Function", args: new object[] { pdfGeneratorFunction })
                .AddTypeActivatedCheck<AzureFunctionHealthCheck>("Text Extractor Function", args: new object[] { textExtractorFunction })
                .AddCheck<DDei.Health.DdeiClientHealthCheck>("DDEI Document Extraction Service");
        }
    }
}