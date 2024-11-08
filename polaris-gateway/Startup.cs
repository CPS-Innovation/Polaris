using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Common.Telemetry;
using Common.Wrappers;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Handlers;
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


[assembly: FunctionsStartup(typeof(PolarisGateway.Startup))]

namespace PolarisGateway
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
#if DEBUG
            // https://stackoverflow.com/questions/54435551/invalidoperationexception-idx20803-unable-to-obtain-configuration-from-pii
#endif
            IdentityModelEventSource.ShowPII = true;
            IdentityModelEventSource.LogCompleteSecurityArtifact = true;
            var services = builder.Services;

            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton(_ => new ConfigurationManager<OpenIdConnectConfiguration>(
                $"https://login.microsoftonline.com/{Environment.GetEnvironmentVariable(OAuthSettings.TenantId)}/v2.0/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever()));
            services.AddSingleton<IAuthorizationValidator, AuthorizationValidator>();
            services.AddSingleton<IJsonConvertWrapper, JsonConvertWrapper>();

            services.AddHttpClient<ICoordinatorClient, CoordinatorClient>(client =>
            {
                client.BaseAddress = new Uri(GetValueFromConfig(Configuration, ConfigurationKeys.PipelineCoordinatorBaseUrl));
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            }).AddPolicyHandler(GetRetryPolicy());

            services.AddDdeiClient(Configuration);

            services.AddSingleton<IRedactPdfRequestMapper, RedactPdfRequestMapper>();
            services.AddSingleton<ITelemetryAugmentationWrapper, TelemetryAugmentationWrapper>();
            services.AddSingleton<ITelemetryClient, TelemetryClient>();
            services.AddSingleton<IUnhandledExceptionHandler, UnhandledExceptionHandler>();
            services.AddSingleton<IInitializationHandler, InitializationHandler>();
            services.AddSingleton<IModifyDocumentRequestMapper, ModifyDocumentRequestMapper>();
            services.AddSingleton<IReclassifyDocumentRequestMapper, ReclassifyDocumentRequestMapper>();
            services.AddTransient<IRequestFactory, RequestFactory>();

            services.AddSingleton<IDocumentToggleService>(new DocumentToggleService(
              DocumentToggleService.ReadConfig()
            ));
            services.AddSingleton<IOcrService, OcrService>();
            services.AddSingleton<IComputerVisionClientFactory, ComputerVisionClientFactory>();

            services.AddSingleton<IPdfGeneratorRequestFactory, PdfGeneratorRequestFactory>();
            services.AddHttpClient<IPdfGeneratorClient, PdfGeneratorClient>(client =>
            {
                client.BaseAddress = new Uri(GetValueFromConfig(Configuration, ConfigurationKeys.PipelineRedactPdfBaseUrl));
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            }).AddPolicyHandler(GetRetryPolicy());
			services.AddHttpClient<IPdfThumbnailGeneratorClient, PdfThumbnailGeneratorClient>(client =>
            {
                client.BaseAddress = new Uri(GetValueFromConfig(Configuration, ConfigurationKeys.PdfThumbnailGeneratorBaseUrl));
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            }).AddPolicyHandler(GetRetryPolicy());

            services.AddBlobStorageWithDefaultAzureCredential(Configuration);
            services.AddPiiService();
            services.AddArtefactService();
            services.AddDdeiOrchestrationService();
        }

        private static string GetValueFromConfig(IConfiguration configuration, string secretName)
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
                // Skip retry if the custom header is present
                if (request.Headers.Contains("X-Skip-Retry"))
                {
                    return false;
                }

                // #27567 - retry on 404 as well as 5xx as coordinator can return 404 when the entity is not found in the durable entity store
                return response.StatusCode == HttpStatusCode.NotFound || response.StatusCode >= HttpStatusCode.InternalServerError;
            }


            return Policy
                .HandleResult<HttpResponseMessage>((result) => ShouldRetry(result.RequestMessage, result))
                .WaitAndRetryAsync(delay);
        }
    }
}
