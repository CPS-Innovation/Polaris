using Azure.Identity;
using Azure.Storage.Blobs;
using Common.Clients;
using Common.Clients.Contracts;
using Common.Constants;
using Common.Domain.Extensions;
using Common.Factories;
using Common.Factories.Contracts;
using Common.Health;
using Common.Mappers;
using Common.Mappers.Contracts;
using Common.Services.SasGeneratorService;
using Common.Validators;
using Common.Validators.Contracts;
using Common.Wrappers;
using Common.Wrappers.Contracts;
using Ddei.Clients;
using Ddei.Factories;
using Ddei.Factories.Contracts;
using Ddei.Options;
using Ddei.Services;
using Ddei.Services.Contract;
using Gateway.Clients.PolarisPipeline;
using Gateway.Clients.PolarisPipeline.Contracts;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using PolarisGateway.CaseDataImplementations.Ddei.Mappers;
using PolarisGateway.CaseDataImplementations.Ddei.Services;
using PolarisGateway.common.Mappers.Contracts;
using PolarisGateway.Factories;
using PolarisGateway.Factories.Contracts;
using PolarisGateway.Mappers;
using PolarisGateway.Services;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http.Headers;

[assembly: FunctionsStartup(typeof(PolarisGateway.Startup))]

namespace PolarisGateway
{
    [ExcludeFromCodeCoverage]
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
#if DEBUG
            // https://stackoverflow.com/questions/54435551/invalidoperationexception-idx20803-unable-to-obtain-configuration-from-pii
            IdentityModelEventSource.ShowPII = true;
#endif

            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
#if DEBUG
                .SetBasePath(Directory.GetCurrentDirectory())
#endif
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .Build();

            builder.Services.AddSingleton<IConfiguration>(configuration);
            builder.Services.AddTransient<IPipelineClientRequestFactory, PipelineClientRequestFactory>();
            builder.Services.AddTransient<IAuthorizationValidator, AuthorizationValidator>();
            builder.Services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            builder.Services.AddTransient<ITriggerCoordinatorResponseFactory, TriggerCoordinatorResponseFactory>();
            builder.Services.AddTransient<ITrackerUrlMapper, TrackerUrlMapper>();
            builder.Services.AddTransient<ICmsAuthValuesFactory, CmsAuthValuesFactory>();

            builder.Services.AddHttpClient<IPipelineClient, PipelineClient>(client =>
            {
                client.BaseAddress = new Uri(GetValueFromConfig(configuration, PipelineSettings.PipelineCoordinatorBaseUrl));
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });

            builder.Services.AddTransient<IRedactPdfRequestMapper, RedactPdfRequestMapper>();

            // DDEI
            builder.Services.AddTransient<ICaseDataArgFactory, CaseDataArgFactory>();
            builder.Services.AddTransient<ICaseDataService, DdeiService>();
            builder.Services.AddTransient<IDocumentService, DdeiService>();
            builder.Services.AddTransient<ICmsModernTokenService, DdeiService>();
            builder.Services.AddTransient<IDdeiClientRequestFactory, DdeiClientRequestFactory>();
            builder.Services.AddOptions<DdeiOptions>().Configure<IConfiguration>((settings, _) =>
            {
                configuration.GetSection("ddei").Bind(settings);
            });
            builder.Services.AddHttpClient<IDdeiClient, DdeiClient>((client) =>
            {
                var options = configuration.GetSection("ddei").Get<DdeiOptions>();
                client.BaseAddress = new Uri(options.BaseUrl);
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });
            builder.Services.AddTransient<ICaseDetailsMapper, CaseDetailsMapper>();
            builder.Services.AddTransient<ICaseDocumentsMapper, CaseDocumentsMapper>();

            BuildHealthChecks(builder);
        }

        // see https://www.davidguida.net/azure-api-management-healthcheck/ for pattern
        // Microsoft.Extensions.Diagnostics.HealthChecks Nuget downgraded to lower release to get package to work
        private static void BuildHealthChecks(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();

            var pipelineCoordinator = "pipelineCoordinator";
            builder.Services.AddHttpClient(pipelineCoordinator, client =>
            {
                string url = Environment.GetEnvironmentVariable("PolarisPipelineCoordinatorBaseUrl");
                client.BaseAddress = new Uri(url.GetBaseUrl());
                client.DefaultRequestHeaders.Add("Cms-Auth-Values", AuthenticatedHealthCheck.CmsAuthValue);
                client.DefaultRequestHeaders.Add("Correlation-Id", AuthenticatedHealthCheck.CorrelationId.ToString());
            });

            var pdfFunctions = "pdfFunctions";
            builder.Services.AddHttpClient(pdfFunctions, client =>
            {
                string url = Environment.GetEnvironmentVariable("PolarisPipelineRedactPdfBaseUrl");
                client.BaseAddress = new Uri(url.GetBaseUrl());
                client.DefaultRequestHeaders.Add("Cms-Auth-Values", AuthenticatedHealthCheck.CmsAuthValue);
                client.DefaultRequestHeaders.Add("Correlation-Id", AuthenticatedHealthCheck.CorrelationId.ToString());
            });

            var ddeiClient = "ddeiClient";

            builder.Services.AddHealthChecks()
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
