using Common.Constants;
using Common.Domain.Extensions;
using Common.Factories;
using Common.Factories.Contracts;
using Common.Health;
using Common.Mappers;
using Common.Mappers.Contracts;
using Common.Validators;
using Common.Validators.Contracts;
using Common.Wrappers;
using Common.Wrappers.Contracts;
using Gateway.Clients.PolarisPipeline;
using Gateway.Clients.PolarisPipeline.Contracts;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using PolarisGateway.common.Mappers.Contracts;
using PolarisGateway.Factories;
using PolarisGateway.Factories.Contracts;
using PolarisGateway.Mappers;
using Common.Telemetry.Wrappers;
using Common.Telemetry.Wrappers.Contracts;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http.Headers;
using Ddei.Services.Extensions;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Common.Configuration;
using Common.Telemetry.Contracts;
using Common.Telemetry;
using Common.Streaming;

[assembly: FunctionsStartup(typeof(PolarisGateway.Startup))]

namespace PolarisGateway
{
    [ExcludeFromCodeCoverage]
    internal class Startup : BaseDependencyInjectionStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
#if DEBUG
            // https://stackoverflow.com/questions/54435551/invalidoperationexception-idx20803-unable-to-obtain-configuration-from-pii
            IdentityModelEventSource.ShowPII = true;
#endif
            var services = builder.Services;

            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<IPipelineClientRequestFactory, PipelineClientRequestFactory>();
            services.AddSingleton<IHttpResponseMessageStreamFactory, HttpResponseMessageStreamFactory>();
            services.AddSingleton(_ =>
            {
                // as per https://github.com/dotnet/aspnetcore/issues/43220, there is guidance to only have one instance of ConfigurationManager
                return new ConfigurationManager<OpenIdConnectConfiguration>(
                    $"https://sts.windows.net/{Environment.GetEnvironmentVariable(OAuthSettings.TenantId)}/.well-known/openid-configuration",
                    new OpenIdConnectConfigurationRetriever(),
                    new HttpDocumentRetriever());
            });
            services.AddTransient<IAuthorizationValidator, AuthorizationValidator>();
            services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            services.AddTransient<ITriggerCoordinatorResponseFactory, TriggerCoordinatorResponseFactory>();
            services.AddTransient<ITrackerUrlMapper, TrackerUrlMapper>();
            services.AddTransient<IPipelineClient, PipelineClient>();

            var pipelineCoordinatorBaseUrl = GetValueFromConfig(Configuration, PipelineSettings.PipelineCoordinatorBaseUrl);
            var pipelineCoordinatorLowlevelBaseUrl = pipelineCoordinatorBaseUrl.Replace("/api", string.Empty);
            services.AddHttpClient(nameof(PipelineClient), client =>
            {
                client.BaseAddress = new Uri(pipelineCoordinatorBaseUrl);
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });
            services.AddHttpClient($"Lowlevel{nameof(PipelineClient)}", client =>
            {
                client.BaseAddress = new Uri(pipelineCoordinatorLowlevelBaseUrl);
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });

            services.AddTransient<IRedactPdfRequestMapper, RedactPdfRequestMapper>();

            services.AddDdeiClient(Configuration);
            services.AddSingleton<ITelemetryAugmentationWrapper, TelemetryAugmentationWrapper>();
            services.AddSingleton<ITelemetryClient, TelemetryClient>();
            BuildHealthChecks(services);
        }

        // see https://www.davidguida.net/azure-api-management-healthcheck/ for pattern
        // Microsoft.Extensions.Diagnostics.HealthChecks Nuget downgraded to lower release to get package to work
        private static void BuildHealthChecks(IServiceCollection services)
        {
            services.AddHttpClient();

            var pipelineCoordinator = "pipelineCoordinator";
            Uri uri = new Uri(Environment.GetEnvironmentVariable("PolarisPipelineCoordinatorBaseUrl"));
            services.AddHttpClient(pipelineCoordinator, client =>
            {
                string url = Environment.GetEnvironmentVariable("PolarisPipelineCoordinatorBaseUrl");
                client.BaseAddress = new Uri(url.GetBaseUrl());
                client.DefaultRequestHeaders.Add("Cms-Auth-Values", AuthenticatedHealthCheck.CmsAuthValue);
                client.DefaultRequestHeaders.Add("Correlation-Id", AuthenticatedHealthCheck.CorrelationId.ToString());
            });

            var pdfFunctions = "pdfFunctions";
            services.AddHttpClient(pdfFunctions, client =>
            {
                string url = Environment.GetEnvironmentVariable("PolarisPipelineRedactPdfBaseUrl");
                client.BaseAddress = new Uri(url.GetBaseUrl());
                client.DefaultRequestHeaders.Add("Cms-Auth-Values", AuthenticatedHealthCheck.CmsAuthValue);
                client.DefaultRequestHeaders.Add("Correlation-Id", AuthenticatedHealthCheck.CorrelationId.ToString());
            });

            var ddeiClient = "ddeiClient";
            services.AddHealthChecks()
                .AddCheck<DdeiClientHealthCheck>(ddeiClient)
                .AddTypeActivatedCheck<AzureFunctionHealthCheck>("Pipeline co-ordinator", args: new object[] { pipelineCoordinator });
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
    }
}
