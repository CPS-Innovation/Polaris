// <copyright file="ServiceExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PolarisGateway.ApplicationStartup
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Common.Clients.PdfGenerator;
    using Common.Factories.ComputerVisionClientFactory;
    using Common.Services.BlobStorage;
    using Common.Services.DocumentToggle;
    using Common.Services.OcrService;
    using Common.Services.PiiService;
    using Common.Telemetry;
    using Common.Wrappers;
    using Cps.Fct.Hk.Ui.Interfaces;
    using Cps.Fct.Hk.Ui.Services;
    using Ddei.Extensions;
    using global::Services;
    using MasterDataServiceClient;
    using MasterDataServiceClient.Configuration;
    using MasterDataServiceClient.Extensions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Protocols;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using PolarisGateway.Clients.Coordinator;
    using PolarisGateway.Clients.PdfThumbnailGenerator;
    using PolarisGateway.Mappers;
    using PolarisGateway.Services.Artefact;
    using PolarisGateway.Services.DdeiOrchestration;
    using PolarisGateway.Validators;
    using Polly;
    using Polly.Contrib.WaitAndRetry;

    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
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

            services.AddHttpClient<ICoordinatorClient, CoordinatorClient>(client =>
            {
                client.BaseAddress = new Uri(GetValueFromConfig(configuration, ConfigurationKeys.PipelineCoordinatorBaseUrl));
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            })
                .AddPolicyHandler(RetryPolicy);

            services.AddDdeiClientGateway(configuration);

            services.AddSingleton<IRedactPdfRequestMapper, RedactPdfRequestMapper>();
            services.AddSingleton<ITelemetryClient, TelemetryClient>();
            services.AddSingleton<IModifyDocumentRequestMapper, ModifyDocumentRequestMapper>();
            services.AddSingleton<IReclassifyDocumentRequestMapper, ReclassifyDocumentRequestMapper>();
            services.AddTransient<IRequestFactory, RequestFactory>();

            services.AddSingleton<IDocumentToggleService>(new DocumentToggleService(
              DocumentToggleService.ReadConfig()));
            services.AddSingleton<IOcrService, OcrService>();
            services.AddSingleton<IComputerVisionClientFactory, ComputerVisionClientFactory>();

            services.AddSingleton<IPdfGeneratorRequestFactory, PdfGeneratorRequestFactory>();
            services.AddHttpClient<IPdfGeneratorClient, PdfGeneratorClient>(client =>
            {
                client.BaseAddress = new Uri(GetValueFromConfig(configuration, ConfigurationKeys.PipelineRedactPdfBaseUrl));
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            })
                .AddPolicyHandler(RetryPolicy);
            services.AddHttpClient<IPdfThumbnailGeneratorClient, PdfThumbnailGeneratorClient>(client =>
            {
                client.BaseAddress = new Uri(GetValueFromConfig(configuration, ConfigurationKeys.PdfThumbnailGeneratorBaseUrl));
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            })
                .AddPolicyHandler(RetryPolicy);

            services.AddBlobStorageWithDefaultAzureCredential(configuration);
            services.AddPiiService();
            services.AddArtefactService();
            services.AddDdeiOrchestrationService();

            // House keeping.
            services.AddSingleton<ICaseInfoService, CaseInfoService>();
            services.AddSingleton<ICookieService, CookieService>();
            services.AddSingleton<IMdsApiClientFactory, MdsApiClientFactory>();

            // Register MDS client options.
            services.AddServiceOptions<MdsClientOptions>(MdsClientOptions.DefaultSectionName);
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
}
