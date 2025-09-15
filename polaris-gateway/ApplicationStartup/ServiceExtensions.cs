using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Common.Telemetry;
using Common.Wrappers;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Mappers;
using PolarisGateway.Validators;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Ddei.Extensions;
using Common.Services.DocumentToggle;
using Common.Services.OcrService;
using Common.Factories.ComputerVisionClientFactory;
using Common.Clients.PdfGenerator;
using Common.Services.BlobStorage;
using Common.Services.PiiService;
using PolarisGateway.Clients.PdfThumbnailGenerator;
using PolarisGateway.Services.Artefact;
using PolarisGateway.Services.DdeiOrchestration;
using System.Net.Http;
using System;

namespace PolarisGateway.ApplicationStartup;

public static class ServiceExtensions
{
    private const int RetryAttempts = 2;
    private const int FirstRetryDelaySeconds = 1;

    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        var isLocal = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
        var sp = services.BuildServiceProvider();
        var configuration = sp.GetService<IConfiguration>();

        services.AddSingleton<Microsoft.ApplicationInsights.TelemetryClient, Microsoft.ApplicationInsights.TelemetryClient>();
        services.AddSingleton(configuration);
        services.AddSingleton(_ => new ConfigurationManager<OpenIdConnectConfiguration>(
                $"https://sts.windows.net/{Environment.GetEnvironmentVariable(OAuthSettings.TenantId)}/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever()));
        services.AddSingleton<IAuthorizationValidator, AuthorizationValidator>();
        services.AddSingleton<IJsonConvertWrapper, JsonConvertWrapper>();

        services.AddHttpClientWitDefaults<ICoordinatorClient, CoordinatorClient>(configuration, ConfigurationKeys.PipelineCoordinatorBaseUrl, ConfigurationKeys.CoordinatorClientTimeoutSeconds);

        services.AddDdeiClientGateway(configuration);

        services.AddSingleton<IRedactPdfRequestMapper, RedactPdfRequestMapper>();
        services.AddSingleton<ITelemetryClient, TelemetryClient>();
        services.AddSingleton<IModifyDocumentRequestMapper, ModifyDocumentRequestMapper>();
        services.AddSingleton<IReclassifyDocumentRequestMapper, ReclassifyDocumentRequestMapper>();
        services.AddTransient<IRequestFactory, RequestFactory>();

        services.AddSingleton<IDocumentToggleService>(new DocumentToggleService(DocumentToggleService.ReadConfig()));
        services.AddSingleton<IOcrService, OcrService>();
        services.AddSingleton<IComputerVisionClientFactory, ComputerVisionClientFactory>();

        services.AddSingleton<IPdfGeneratorRequestFactory, PdfGeneratorRequestFactory>();

        services.AddHttpClientWitDefaults<IPdfGeneratorClient, PdfGeneratorClient>(configuration, ConfigurationKeys.PipelineRedactPdfBaseUrl, ConfigurationKeys.PdfGeneratorClientTimeoutSeconds);
        services.AddHttpClientWitDefaults<IPdfThumbnailGeneratorClient, PdfThumbnailGeneratorClient>(configuration, ConfigurationKeys.PdfThumbnailGeneratorBaseUrl, ConfigurationKeys.PdfThumbnailGeneratorClientTimeoutSeconds);

        services.AddBlobStorageWithDefaultAzureCredential(configuration);
        services.AddPiiService();
        services.AddArtefactService();
        services.AddDdeiOrchestrationService();
        return services;
    }

    public static IServiceCollection AddHttpClientWitDefaults<TInterface, TImplementation>(this IServiceCollection services, IConfiguration configuration, string baseUrlKey, string timeoutSecondsKey)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.AddHttpClient<TInterface, TImplementation>(client => 
        {
            client.BaseAddress = new Uri(GetValueFromConfig(configuration, baseUrlKey));
            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            var hasTimeout = int.TryParse(GetValueFromConfig(configuration, timeoutSecondsKey), out var timeoutSeconds);
            client.Timeout = TimeSpan.FromSeconds(hasTimeout ? timeoutSeconds : 100);
        })
        .AddPolicyHandler(RetryPolicy);
        return services;
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


    private static IAsyncPolicy<HttpResponseMessage> RetryPolicy =>
        // https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-http-call-retries-exponential-backoff-polly#add-a-jitter-strategy-to-the-retry-policy
        Policy
            .HandleResult<HttpResponseMessage>((result) => ShouldRetry(result.RequestMessage, result))
            .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(
                medianFirstRetryDelay: TimeSpan.FromSeconds(FirstRetryDelaySeconds),
                retryCount: RetryAttempts));

    private static bool ShouldRetry(HttpRequestMessage request, HttpResponseMessage response)
    {
        // Skip retry if the custom header is present
        if (request.Headers.Contains("X-Skip-Retry"))
        {
            return false;
        }

        // #27567 - retry on 404 as well as 5xx as coordinator can return 404 when the entity is not found in the durable entity store
        return response.StatusCode == HttpStatusCode.NotFound || response.StatusCode >= HttpStatusCode.InternalServerError;
    }
}