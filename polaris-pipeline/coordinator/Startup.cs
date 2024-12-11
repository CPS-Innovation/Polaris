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
using Common.Factories.ComputerVisionClientFactory;
using coordinator.Factories.UploadFileNameFactory;
using coordinator.Functions.DurableEntity.Entity.Mapper;
using coordinator.Mappers;
using coordinator.Services.ClearDownService;
using Common.Services.DocumentToggle;
using Common.Services.OcrService;
using coordinator.Validators;
using Common.Domain.Validators;
using Common.Dto.Request;
using Common.Handlers;
using Common.Services.BlobStorage;
using Common.Streaming;
using Common.Telemetry;
using Common.Wrappers;
using Ddei.Extensions;
using FluentValidation;
using System.Net.Http;
using Polly;
using Polly.Contrib.WaitAndRetry;
using System.Net;

using PdfGenerator = Common.Clients.PdfGenerator;
using TextExtractor = coordinator.Clients.TextExtractor;
using PdfRedactor = coordinator.Clients.PdfRedactor;
using Common.Services.PiiService;
using Common.Services.RenderHtmlService;

[assembly: FunctionsStartup(typeof(Startup))]
namespace coordinator
{
    [ExcludeFromCodeCoverage]
    internal class Startup : FunctionsStartup
    {
        protected IConfigurationRoot Configuration { get; set; }
        private const int RetryAttempts = 2;
        private const int FirstRetryDelaySeconds = 1;

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
            BuildOcrService(services, Configuration);

            services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            services.AddTransient<IValidatorWrapper<DocumentPayload>, ValidatorWrapper<DocumentPayload>>();
            services.AddSingleton<IConvertModelToHtmlService, ConvertModelToHtmlService>();
            services.AddTransient<TextExtractor.IRequestFactory, TextExtractor.RequestFactory>();
            services.AddTransient<PdfGenerator.IPdfGeneratorRequestFactory, PdfGenerator.PdfGeneratorRequestFactory>();
            services.AddTransient<PdfRedactor.IRequestFactory, PdfRedactor.RequestFactory>();
            services.AddTransient<TextExtractor.ISearchDtoContentFactory, TextExtractor.SearchDtoContentFactory>();
            services.AddTransient<IQueryConditionFactory, QueryConditionFactory>();
            services.AddTransient<IExceptionHandler, ExceptionHandler>();
            services.AddSingleton<IHttpResponseMessageStreamFactory, HttpResponseMessageStreamFactory>();
            services.AddTransient<IComputerVisionClientFactory, ComputerVisionClientFactory>();
            services.AddBlobStorageWithDefaultAzureCredential(Configuration);
            services.AddPiiService();

            services.AddSingleton<IUploadFileNameFactory, UploadFileNameFactory>();
            services.AddHttpClient<PdfGenerator.IPdfGeneratorClient, PdfGenerator.PdfGeneratorClient>(client =>
            {
                client.BaseAddress = new Uri(GetValueFromConfig(Configuration, ConfigKeys.PipelineRedactPdfBaseUrl));
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            }).AddPolicyHandler(GetRetryPolicy());
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

            services.AddTransient<ISearchFilterDocumentMapper, SearchFilterDocumentMapper>();
            services.AddScoped<IValidator<RedactPdfRequestWithDocumentDto>, RedactPdfRequestWithDocumentValidator>();
            services.AddScoped<IValidator<RedactPdfRequestDto>, RedactPdfRequestValidator>();
            services.AddScoped<IValidator<AddDocumentNoteDto>, DocumentNoteValidator>();
            services.AddScoped<IValidator<RenameDocumentDto>, RenameDocumentValidator>();
            services.AddScoped<IValidator<ReclassifyDocumentDto>, ReclassifyDocumentValidator>();
            services.AddScoped<IValidator<ModifyDocumentWithDocumentDto>, ModifyDocumentWithDocumentValidator>();
            services.AddSingleton<ICmsDocumentsResponseValidator, CmsDocumentsResponseValidator>();
            services.AddSingleton<IClearDownService, ClearDownService>();
            services.AddTransient<IOrchestrationProvider, OrchestrationProvider>();


            services.RegisterCoordinatorMapsterConfiguration();
            services.AddDdeiClient(Configuration);
            // services.AddTransient<IDocumentToggleService, DocumentToggleService>();
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

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            // https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-http-call-retries-exponential-backoff-polly#add-a-jitter-strategy-to-the-retry-policy
            var delay = Backoff.DecorrelatedJitterBackoffV2(
                medianFirstRetryDelay: TimeSpan.FromSeconds(FirstRetryDelaySeconds),
                retryCount: RetryAttempts);

            static bool ShouldRetry(HttpRequestMessage request, HttpResponseMessage response)
            {

                return response.StatusCode >= HttpStatusCode.InternalServerError;
            }

            return Policy
                .HandleResult<HttpResponseMessage>((result) => ShouldRetry(result.RequestMessage, result))
                .WaitAndRetryAsync(delay);
        }

        private static void BuildOcrService(IServiceCollection services, IConfigurationRoot configuration)
        {
#if DEBUG
            if (configuration.IsSettingEnabled(MockOcrService.MockOcrServiceSetting))
            {
                services.AddSingleton<IOcrService, MockOcrService>();
            }
            else
            {
                services.AddSingleton<IOcrService, OcrService>();
            }
#else
            services.AddSingleton<IOcrService, OcrService>();
#endif
        }
    }
}