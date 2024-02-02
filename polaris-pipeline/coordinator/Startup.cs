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
using coordinator.Clients.Contracts;
using coordinator.Clients;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Common.Health;
using Common.Wrappers.Contracts;
using FluentValidation;
using Common.Domain.Validators;
using Common.Dto.Request;
using Ddei.Services.Extensions;
using Common.Handlers.Contracts;
using Common.Handlers;
using coordinator.Domain;
using coordinator.Services.RenderHtmlService;
using coordinator.Domain.Mapper;
using coordinator.Services.RenderHtmlService.Contract;
using Common.Telemetry.Contracts;
using Common.Telemetry;
using coordinator.Providers;
using coordinator.Validators;
using coordinator.Services.DocumentToggle;

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
            services.AddTransient<IQueryConditionFactory, QueryConditionFactory>();
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

            services.AddTransient<ISearchFilterDocumentMapper, SearchFilterDocumentMapper>();
            services.AddTransient<IPipelineClientSearchRequestFactory, PipelineClientSearchRequestFactory>();
            services.AddScoped<IValidator<RedactPdfRequestDto>, RedactPdfRequestValidator>();
            services.AddSingleton<ICmsDocumentsResponseValidator, CmsDocumentsResponseValidator>();
            builder.Services.AddTransient<IOrchestrationProvider, OrchestrationProvider>();

            services.RegisterMapsterConfiguration();
            services.AddDdeiClient(Configuration);
            services.AddTransient<IDocumentToggleService, DocumentToggleService>();
            services.AddSingleton<IDocumentToggleService>(new DocumentToggleService(
              DocumentToggleService.ReadConfig()
            ));

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
                .AddCheck<PolarisBlobStorageServiceHealthCheck>("PolarisBlobStorageService");

            healthChecks
                .AddTypeActivatedCheck<AzureFunctionHealthCheck>("PDF Generator Function", args: new object[] { pdfGeneratorFunction });
        }
    }
}