using Common.Domain.Validators;
using Common.Dto.Request;
using Common.Factories.ComputerVisionClientFactory;
using Common.Handlers;
using Common.Mappers;
using Common.Services.BlobStorage;
using Common.Services.DocumentToggle;
using Common.Services.OcrService;
using Common.Services.PiiService;
using Common.Services.RenderHtmlService;
using Common.Streaming;
using Common.Telemetry;
using Common.Wrappers;
using coordinator.Constants;
using coordinator.Durable.Payloads;
using coordinator.Durable.Providers;
using coordinator.Factories.UploadFileNameFactory;
using coordinator.Functions.DurableEntity.Entity.Mapper;
using coordinator.Mappers;
using coordinator.Services;
using coordinator.Services.ClearDownService;
using coordinator.Validators;
using Ddei.Extensions;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Contrib.WaitAndRetry;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using PdfGenerator = Common.Clients.PdfGenerator;
using PdfRedactor = coordinator.Clients.PdfRedactor;
using TextExtractor = coordinator.Clients.TextExtractor;

namespace coordinator.ApplicationStartup;

public static class ServiceExtensions
{
    private const int RetryAttempts = 2;
    private const int FirstRetryDelaySeconds = 1;

    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        var isLocal = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
        var sp = services.BuildServiceProvider();
        var configuration = sp.GetService<IConfiguration>();

        services.AddSingleton(configuration);
        BuildOcrService(services, configuration);

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
        services.AddBlobStorageWithDefaultAzureCredential(configuration);
        services.AddPiiService();

        services.AddSingleton<IUploadFileNameFactory, UploadFileNameFactory>();
        services.AddHttpClientWithDefaults<PdfGenerator.IPdfGeneratorClient, PdfGenerator.PdfGeneratorClient>(configuration, ConfigKeys.PipelineRedactPdfBaseUrl, ConfigKeys.PdfGeneratorClientTimeoutSeconds).AddPolicyHandler(GetRetryPolicy);
        services.AddHttpClientWithDefaults<PdfRedactor.IPdfRedactorClient, PdfRedactor.PdfRedactorClient>(configuration, ConfigKeys.PipelineRedactorPdfBaseUrl, ConfigKeys.PdfRedactorClientTimeoutSeconds);
        services.AddHttpClientWithDefaults<TextExtractor.ITextExtractorClient, TextExtractor.TextExtractorClient>(configuration, ConfigKeys.PipelineTextExtractorBaseUrl, ConfigKeys.TextExtractorClientTimeoutSeconds);

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
        services.AddDdeiClientGateway(configuration);
        // services.AddTransient<IDocumentToggleService, DocumentToggleService>();
        services.AddSingleton<IDocumentToggleService>(new DocumentToggleService(DocumentToggleService.ReadConfig()));

        services.AddSingleton<ITelemetryClient, TelemetryClient>();
        services.AddSingleton<ICaseDurableEntityMapper, CaseDurableEntityMapper>();
        services.AddSingleton<IStateStorageService, StateStorageService>();
        services.AddSingleton<IRedactionSearchDtoMapper, RedactionSearchDtoMapper>();
        return services;
    }

    public static IHttpClientBuilder AddHttpClientWithDefaults<TInterface, TImplementation>(this IServiceCollection services, IConfiguration configuration, string baseUrlKey, string timeoutKey) 
        where TInterface : class
        where TImplementation : class, TInterface
    {
        return services.AddHttpClient<TInterface, TImplementation>(client =>
        {
            client.BaseAddress = new Uri(GetValueFromConfig(configuration, baseUrlKey));
            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            var hasTimeout = int.TryParse(configuration[timeoutKey], out var timeout);
            client.Timeout = TimeSpan.FromSeconds(hasTimeout ? timeout : 100);
        });
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

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy =>
        // https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-http-call-retries-exponential-backoff-polly#add-a-jitter-strategy-to-the-retry-policy
        Policy
            .HandleResult<HttpResponseMessage>((result) => ShouldRetry(result.RequestMessage, result))
            .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(
            medianFirstRetryDelay: TimeSpan.FromSeconds(FirstRetryDelaySeconds),
            retryCount: RetryAttempts));

    private static bool ShouldRetry(HttpRequestMessage _, HttpResponseMessage response) =>
        response.StatusCode >= HttpStatusCode.InternalServerError;

    private static void BuildOcrService(IServiceCollection services, IConfiguration configuration)
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