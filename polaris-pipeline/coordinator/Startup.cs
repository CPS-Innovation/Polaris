using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http.Headers;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using coordinator;
using coordinator.Constants;
using coordinator.Durable.Payloads;
using coordinator.Durable.Providers;
using coordinator.Functions.DurableEntity.Entity.Mapper;
using coordinator.Mappers;
using coordinator.Services.CleardownService;
using coordinator.Services.DocumentToggle;
using coordinator.Services.RenderHtmlService;
using coordinator.Services.TextExtractService;
using coordinator.Validators;
using Common.Domain.Validators;
using Common.Dto.Request;
using Common.Handlers;
using Common.Services;
using Common.Streaming;
using Common.Telemetry;
using Common.Wrappers;
using Ddei.Services.Extensions;
using FluentValidation;

using PdfGenerator = coordinator.Clients.PdfGenerator;
using TextExtractor = coordinator.Clients.TextExtractor;
using PdfRedactor = coordinator.Clients.PdfRedactor;

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
            services.AddTransient<TextExtractor.ISearchDtoContentFactory, TextExtractor.SearchDtoContentFactory>();
            services.AddTransient<IQueryConditionFactory, QueryConditionFactory>();
            services.AddTransient<IExceptionHandler, ExceptionHandler>();
            services.AddSingleton<IHttpResponseMessageStreamFactory, HttpResponseMessageStreamFactory>();
            services.AddBlobStorageWithDefaultAzureCredential(Configuration);

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
            services.AddScoped<IValidator<RedactPdfRequestDto>, RedactPdfRequestValidator>();
            services.AddScoped<IValidator<AddDocumentNoteDto>, DocumentNoteValidator>();
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