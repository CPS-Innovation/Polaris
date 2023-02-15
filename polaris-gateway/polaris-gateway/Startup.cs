using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PolarisGateway.Clients.PolarisPipeline;
using PolarisGateway.Domain.PolarisPipeline;
using PolarisGateway.Domain.Validators;
using PolarisGateway.Factories;
using PolarisGateway.CaseDataImplementations.Ddei.Clients;
using PolarisGateway.CaseDataImplementations.Ddei.Options;
using PolarisGateway.CaseDataImplementations.Ddei.Services;
using PolarisGateway.Mappers;
using PolarisGateway.Services;
using PolarisGateway.Wrappers;
using PolarisGateway.CaseDataImplementations.Ddei.Factories;
using PolarisGateway.CaseDataImplementations.Ddei.Mappers;
using Microsoft.IdentityModel.Logging;

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
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .Build();

            builder.Services.AddSingleton<IConfiguration>(configuration);
            builder.Services.AddOptions<SearchClientOptions>().Configure<IConfiguration>((settings, _) =>
            {
                configuration.GetSection("searchClient").Bind(settings);
            });
            builder.Services.AddTransient<IPipelineClientRequestFactory, PipelineClientRequestFactory>();
            builder.Services.AddTransient<IAuthorizationValidator, AuthorizationValidator>();
            builder.Services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            builder.Services.AddTransient<ITriggerCoordinatorResponseFactory, TriggerCoordinatorResponseFactory>();
            builder.Services.AddTransient<ITrackerUrlMapper, TrackerUrlMapper>();
            builder.Services.AddTransient<ISearchIndexClient, SearchIndexClient>();
            builder.Services.AddTransient<ISearchClientFactory, SearchClientFactory>();
            builder.Services.AddTransient<IStreamlinedSearchLineMapper, StreamlinedSearchLineMapper>();
            builder.Services.AddTransient<IStreamlinedSearchWordMapper, StreamlinedSearchWordMapper>();
            builder.Services.AddTransient<IStreamlinedSearchResultFactory, StreamlinedSearchResultFactory>();
            builder.Services.AddTransient<ICmsAuthValuesFactory, CmsAuthValuesFactory>();

            builder.Services.AddHttpClient<IPipelineClient, PipelineClient>(client =>
            {
                client.BaseAddress = new Uri(GetValueFromConfig(configuration, ConfigurationKeys.PipelineCoordinatorBaseUrl));
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });

            builder.Services.AddHttpClient<IRedactionClient, RedactionClient>(client =>
            {
                client.BaseAddress = new Uri(GetValueFromConfig(configuration, ConfigurationKeys.PipelineRedactPdfBaseUrl));
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });

            builder.Services.AddSingleton(_ =>
            {
                const string authInstanceUrl = AuthenticationKeys.AzureAuthenticationInstanceUrl;
                var tenantId = GetValueFromConfig(configuration, ConfigurationKeys.TenantId);
                var clientId = GetValueFromConfig(configuration, ConfigurationKeys.ClientId);
                var clientSecret = GetValueFromConfig(configuration, ConfigurationKeys.ClientSecret);
                var appOptions = new ConfidentialClientApplicationOptions
                {
                    Instance = authInstanceUrl,
                    TenantId = tenantId,
                    ClientId = clientId,
                    ClientSecret = clientSecret
                };

                var authority = $"{authInstanceUrl}{tenantId}/";

                return ConfidentialClientApplicationBuilder.CreateWithApplicationOptions(appOptions).WithAuthority(authority).Build();
            });

            // TODO - remove when Blob handling code moved to Pipeline
            BuildBlobServiceClient(builder, configuration);

            builder.Services.AddTransient<ISasGeneratorService, SasGeneratorService>();
            builder.Services.AddTransient<IBlobSasBuilderWrapper, BlobSasBuilderWrapper>();
            builder.Services.AddTransient<IBlobSasBuilderFactory, BlobSasBuilderFactory>();
            builder.Services.AddTransient<IBlobSasBuilderWrapperFactory, BlobSasBuilderWrapperFactory>();
            builder.Services.AddTransient<IRedactPdfRequestMapper, RedactPdfRequestMapper>();

            builder.Services.AddTransient<ICaseDataArgFactory, CaseDataArgFactory>();

            builder.Services.AddOptions<DdeiOptions>().Configure<IConfiguration>((settings, _) =>
            {
                configuration.GetSection("ddei").Bind(settings);
            });

            builder.Services.AddTransient<ICaseDataService, DdeiService>();
            builder.Services.AddTransient<IDocumentService, DdeiService>();
            builder.Services.AddTransient<ICmsModernTokenService, DdeiService>();
            builder.Services.AddTransient<IDdeiClientRequestFactory, DdeiClientRequestFactory>();
            builder.Services.AddHttpClient<IDdeiClient, DdeiClient>((client) =>
            {
                var options = configuration.GetSection("ddei").Get<DdeiOptions>();
                client.BaseAddress = new Uri(options.BaseUrl);
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });
            builder.Services.AddTransient<ICaseDetailsMapper, CaseDetailsMapper>();
            builder.Services.AddTransient<ICaseDocumentsMapper, CaseDocumentsMapper>();
        }

        private static void BuildBlobServiceClient(IFunctionsHostBuilder builder, IConfigurationRoot configuration)
        {
            builder.Services.AddAzureClients(azureBuilder =>
            {
                azureBuilder.AddBlobServiceClient(new Uri(GetValueFromConfig(configuration, ConfigurationKeys.BlobServiceUrl)))
                    .WithCredential(new DefaultAzureCredential());
            });

            builder.Services.AddTransient((Func<IServiceProvider, IBlobStorageClient>)(serviceProvider =>
            {
                var logger = serviceProvider.GetService<ILogger<BlobStorageClient>>();
                BlobServiceClient blobServiceClient = serviceProvider.GetRequiredService<BlobServiceClient>();
                string blobServiceContainerName = GetValueFromConfig(configuration, ConfigurationKeys.BlobContainerName);
                return new BlobStorageClient(blobServiceClient, blobServiceContainerName, logger);
            }));
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
