using Common.Wrappers;
using Common.Wrappers.Contracts;
using PolarisGateway.Clients.Coordinator;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using PolarisGateway.Mappers;
using PolarisGateway.Validators;
using Common.Telemetry.Wrappers;
using Common.Telemetry.Wrappers.Contracts;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Ddei.Services.Extensions;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Common.Configuration;
using Common.Telemetry.Contracts;
using Common.Telemetry;
using PolarisGateway.Handlers;

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
            services.AddSingleton(_ =>
            {
                // as per https://github.com/dotnet/aspnetcore/issues/43220, there is guidance to only have one instance of ConfigurationManager
                return new ConfigurationManager<OpenIdConnectConfiguration>(
                    $"https://sts.windows.net/{Environment.GetEnvironmentVariable(OAuthSettings.TenantId)}/.well-known/openid-configuration",
                    new OpenIdConnectConfigurationRetriever(),
                    new HttpDocumentRetriever());
            });
            services.AddSingleton<IAuthorizationValidator, AuthorizationValidator>();
            services.AddSingleton<IJsonConvertWrapper, JsonConvertWrapper>();

            services.AddHttpClient<IClient, Client>(client =>
            {
                client.BaseAddress = new Uri(GetValueFromConfig(Configuration, ConfigurationKeys.PipelineCoordinatorBaseUrl));
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });

            services.AddSingleton<IRedactPdfRequestMapper, RedactPdfRequestMapper>();

            services.AddDdeiClient(Configuration);
            services.AddSingleton<ITelemetryAugmentationWrapper, TelemetryAugmentationWrapper>();
            services.AddSingleton<ITelemetryClient, TelemetryClient>();
            services.AddSingleton<IUnhandledExceptionHandler, UnhandledExceptionHandler>();
            services.AddSingleton<IInitializationHandler, InitializationHandler>();
            services.AddTransient<IRequestFactory, RequestFactory>();
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
