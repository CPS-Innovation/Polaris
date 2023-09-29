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
using Common.Dto.Request;
using Ddei.Services.Extensions;
using Common.Handlers.Contracts;
using Common.Handlers;
using coordinator.Domain;
using RenderPcd;
using coordinator.Domain.Mapper;
using Common.Services.RenderHtmlService.Contract;
using Common.Telemetry.Contracts;
using Common.Telemetry;
using coordinator.Providers;
using Microsoft.Extensions.Azure;
using coordinator.Health;

[assembly: FunctionsStartup(typeof(Startup))]
namespace coordinator
{
    [ExcludeFromCodeCoverage]
    internal class Startup : BaseDependencyInjectionStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var services = builder.Services;

            services.AddSingleton<IConfiguration>(Configuration);
            services.AddTransient<IDefaultAzureCredentialFactory, DefaultAzureCredentialFactory>();
            services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            services.AddTransient<IValidatorWrapper<CaseDocumentOrchestrationPayload>, ValidatorWrapper<CaseDocumentOrchestrationPayload>>();
            services.AddSingleton<IGeneratePdfHttpRequestFactory, GeneratePdfHttpRequestFactory>();
            services.AddSingleton<IConvertModelToHtmlService, ConvertModelToHtmlService>();
            services.AddTransient<IPipelineClientRequestFactory, PipelineClientRequestFactory>();
            services.AddTransient<IPipelineClientSearchRequestFactory, PipelineClientSearchRequestFactory>();
            services.AddTransient<IExceptionHandler, ExceptionHandler>();
            services.AddBlobStorageWithDefaultAzureCredential(Configuration);

            services.AddHttpClient<IPdfGeneratorClient, PdfGeneratorClient>(client =>
            {
                client.BaseAddress = new Uri(Configuration.GetValueFromConfig(PipelineSettings.PipelineRedactPdfBaseUrl));
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });
            services.AddHttpClient<ITextExtractorClient, TextExtractorClient>(client =>
            {
                client.BaseAddress = new Uri(Configuration.GetValueFromConfig(PipelineSettings.PipelineTextExtractorBaseUrl));
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });
            var pipelineCoordinatorBaseUrl = Configuration.GetValueFromConfig(PipelineSettings.PipelineCoordinatorBaseUrl);
            var orchestrationLowLevelApiBaseUrl = pipelineCoordinatorBaseUrl.Replace("/api", string.Empty);
            builder.Services.AddHttpClient($"Low-level{nameof(OrchestrationProvider)}", client =>
            {
                client.BaseAddress = new Uri(orchestrationLowLevelApiBaseUrl);
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });

            services.AddTransient<ISearchFilterDocumentMapper, SearchFilterDocumentMapper>();
            services.AddTransient<IRedactPdfRequestMapper, RedactPdfRequestMapper>();
            services.AddTransient<IPipelineClientSearchRequestFactory, PipelineClientSearchRequestFactory>();
            services.AddScoped<IValidator<RedactPdfRequestDto>, RedactPdfRequestValidator>();
            builder.Services.AddTransient<IOrchestrationProvider, OrchestrationProvider>();

            services.RegisterMapsterConfiguration();
            services.AddBlobSasGenerator();
            services.AddSearchClient(Configuration);
            services.AddDdeiClient(Configuration);

            services.AddSingleton<ITelemetryClient, TelemetryClient>();
            BuildHealthChecks(builder, Configuration);
        }

        /// <summary>
        /// see https://www.davidguida.net/azure-api-management-healthcheck/ for pattern
        /// Microsoft.Extensions.Diagnostics.HealthChecks Nuget downgraded to lower release to get package to work
        /// </summary>
        /// <param name="builder"></param>
        private static void BuildHealthChecks(IFunctionsHostBuilder builder, IConfigurationRoot configuration)
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

            var healthChecks = builder.Services.AddHealthChecks();
            healthChecks
                .AddCheck<DDei.Health.DdeiClientHealthCheck>("DDEI")
                .AddCheck<AzureBlobServiceClientHealthCheck>("Azure Blob Service Client")
                .AddCheck<PolarisBlobStorageServiceHealthCheck>("PolarisBlobStorageService")
                .AddCheck<OrchestrationProviderHealthCheck>("OrchestrationProvider");
                //.AddCheck<IDurableOrchestrationClientHealthCheck>("IDurableOrchestrationClient");

            if (!configuration.IsConfigSettingEnabled(FeatureFlags.DisableTextExtractorFeatureFlag))
                healthChecks
                    .AddCheck<AzureSearchClientHealthCheck>("Azure Search Client")
                    .AddTypeActivatedCheck<AzureFunctionHealthCheck>("Text Extractor Function", args: new object[] { textExtractorFunction });

            if (!configuration.IsConfigSettingEnabled(FeatureFlags.DisableConvertToPdfFeatureFlag))
                healthChecks
                    .AddTypeActivatedCheck<AzureFunctionHealthCheck>("PDF Generator Function", args: new object[] { pdfGeneratorFunction });
        }
    }
}