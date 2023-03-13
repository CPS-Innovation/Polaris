using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Common.Configuration;
using Common.Constants;
using Common.Domain.Responses;
using Common.Domain.Extensions;
using Common.Factories;
using Common.Factories.Contracts;
using Common.Mappers;
using Common.Mappers.Contracts;
using Common.Services.DocumentExtractionService;
using Common.Services.DocumentExtractionService.Contracts;
using Common.Services.Extensions;
using Common.Wrappers;
using coordinator;
using coordinator.Factories;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Common.Health;

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
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .Build();

            builder.Services.AddSingleton<IConfiguration>(configuration);

            builder.Services.AddTransient<IDefaultAzureCredentialFactory, DefaultAzureCredentialFactory>();
            builder.Services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            builder.Services.AddSingleton<IGeneratePdfHttpRequestFactory, GeneratePdfHttpRequestFactory>();
            builder.Services.AddSingleton<ITextExtractorHttpRequestFactory, TextExtractorHttpRequestFactory>();
            builder.Services.AddTransient<IHttpRequestFactory, HttpRequestFactory>();
            builder.Services.AddTransient<ICaseDocumentMapper<DdeiCaseDocumentResponse>, DdeiCaseDocumentMapper>();
            builder.Services.AddHttpClient<IDdeiDocumentExtractionService, DdeiDocumentExtractionService>(client =>
            {
                client.BaseAddress = new Uri(configuration.GetValueFromConfig(ConfigKeys.SharedKeys.DocumentsRepositoryBaseUrl));
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });

            builder.Services.AddBlobStorage(configuration);
            builder.Services.AddBlobSasGenerator();
            builder.Services.AddSearchClient(configuration);

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
                .AddCheck<DdeiDocumentExtractionServiceHealthCheck>("DDEI Document Extraction Service");
        }
    }
}