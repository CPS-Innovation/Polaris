using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Common.Configuration;
using Common.Services;
using Common.Wrappers;
using coordinator;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Common.Domain.Validators;
using Common.Dto.Request;
using Ddei.Services.Extensions;
using Common.Handlers;
using coordinator.Constants;
using coordinator.Services.RenderHtmlService;
using coordinator.Mappers;
using Common.Telemetry;
using coordinator.Durable.Providers;
using coordinator.Validators;
using coordinator.Services.DocumentToggle;
using Common.Streaming;
using coordinator.Services.TextExtractService;
using coordinator.Services.CleardownService;
using coordinator.Durable.Payloads;
using coordinator.Functions.DurableEntity.Entity.Mapper;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using PdfGenerator = coordinator.Clients.PdfGenerator;
using TextExtractor = coordinator.Clients.TextExtractor;
using PdfRedactor = coordinator.Clients.PdfRedactor;
using System.IO;
using coordinator.Factories.UploadFileNameFactory;

[assembly: FunctionsStartup(typeof(Startup))]
namespace coordinator
{
    [ExcludeFromCodeCoverage]
    internal class Startup : FunctionsStartup
    {
        protected IConfigurationRoot Configuration { get; set; }

        // https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection#customizing-configuration-sources
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();

            var configurationBuilder = builder.ConfigurationBuilder
                .AddEnvironmentVariables()
#if DEBUG
                .SetBasePath(Directory.GetCurrentDirectory())
#endif
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);

            Configuration = configurationBuilder.Build();
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var services = builder.Services;

            services.AddSingleton<IConfiguration>(Configuration);
            services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            services.AddTransient<IValidatorWrapper<CaseDocumentOrchestrationPayload>, ValidatorWrapper<CaseDocumentOrchestrationPayload>>();
            services.AddSingleton<IConvertModelToHtmlService, ConvertModelToHtmlService>();
            services.AddTransient<TextExtractor.IRequestFactory, TextExtractor.RequestFactory>();
            services.AddTransient<PdfGenerator.IRequestFactory, PdfGenerator.RequestFactory>();
            services.AddTransient<PdfRedactor.IRequestFactory, PdfRedactor.RequestFactory>();
            services.AddTransient<TextExtractor.ISearchDtoContentFactory, TextExtractor.SearchDtoContentFactory>();
            services.AddTransient<IQueryConditionFactory, QueryConditionFactory>();
            services.AddTransient<IExceptionHandler, ExceptionHandler>();
            services.AddSingleton<IHttpResponseMessageStreamFactory, HttpResponseMessageStreamFactory>();
            services.AddBlobStorageWithDefaultAzureCredential(Configuration);

            services.AddSingleton<IUploadFileNameFactory, UploadFileNameFactory>();
            services.AddHttpClient<PdfGenerator.IPdfGeneratorClient, PdfGenerator.PdfGeneratorClient>(client =>
            {
                client.BaseAddress = new Uri(GetValueFromConfig(Configuration, ConfigKeys.PipelineRedactPdfBaseUrl));
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });
            services.AddHttpClient<PdfRedactor.IPdfRedactorClient, PdfRedactor.PdfRedactorClient>(client =>
            {
                client.BaseAddress = new Uri(GetValueFromConfig(Configuration, ConfigKeys.PipelineRedactorPdfBaseUrl));
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });


            services.AddHttpClient<TextExtractor.ITextExtractorClient, TextExtractor.TextExtractorClient>(client =>
            {
                client.BaseAddress = new Uri(GetValueFromConfig(Configuration, ConfigKeys.PipelineTextExtractorBaseUrl));
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });

            services.AddTransient<ITextExtractService, TextExtractService>();
            services.AddTransient<ISearchFilterDocumentMapper, SearchFilterDocumentMapper>();
            services.AddScoped<IValidator<RedactPdfRequestWithDocumentDto>, RedactPdfRequestWithDocumentValidator>();
            services.AddScoped<IValidator<RedactPdfRequestDto>, RedactPdfRequestValidator>();
            services.AddSingleton<ICmsDocumentsResponseValidator, CmsDocumentsResponseValidator>();
            services.AddSingleton<ICleardownService, CleardownService>();
            services.AddTransient<IOrchestrationProvider, OrchestrationProvider>();

            services.RegisterMapsterConfiguration();
            services.AddDdeiClient(Configuration);
            services.AddTransient<IDocumentToggleService, DocumentToggleService>();
            services.AddSingleton<IDocumentToggleService>(new DocumentToggleService(
              DocumentToggleService.ReadConfig()
            ));

            services.AddSingleton<ITelemetryClient, TelemetryClient>();
            services.AddSingleton<ICaseDurableEntityMapper, CaseDurableEntityMapper>();

            services.AddDurableClientFactory();
        }

        public static string GetValueFromConfig(IConfiguration configuration, string secretName)
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