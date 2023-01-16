using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Common.Adapters;
using Common.Constants;
using Common.Domain.Responses;
using Common.Factories;
using Common.Factories.Contracts;
using Common.Handlers;
using Common.Mappers;
using Common.Mappers.Contracts;
using Common.Services.DocumentExtractionService;
using Common.Services.DocumentExtractionService.Contracts;
using Common.Wrappers;
using coordinator;
using coordinator.Factories;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;

[assembly: FunctionsStartup(typeof(Startup))]
namespace coordinator
{
    [ExcludeFromCodeCoverage]
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .Build();

            builder.Services.AddSingleton<IConfiguration>(configuration);
            builder.Services.AddSingleton(_ =>
            {
                const string instance = AuthenticationKeys.AzureAuthenticationInstanceUrl;
                var onBehalfOfTokenTenantId = GetValueFromConfig(configuration, ConfigKeys.SharedKeys.TenantId);
                var onBehalfOfTokenClientId = GetValueFromConfig(configuration, ConfigKeys.SharedKeys.ClientId);
                var onBehalfOfTokenClientSecret = GetValueFromConfig(configuration, ConfigKeys.SharedKeys.ClientSecret);
                var appOptions = new ConfidentialClientApplicationOptions
                {
                    Instance = instance,
                    TenantId = onBehalfOfTokenTenantId,
                    ClientId = onBehalfOfTokenClientId,
                    ClientSecret = onBehalfOfTokenClientSecret
                };

                var authority = $"{instance}{onBehalfOfTokenTenantId}/";

                return ConfidentialClientApplicationBuilder.CreateWithApplicationOptions(appOptions).WithAuthority(authority).Build();
            });
            builder.Services.AddTransient<IAuthorizationValidator, AuthorizationValidator>();
            builder.Services.AddTransient<IIdentityClientAdapter, IdentityClientAdapter>();
            builder.Services.AddTransient<IDefaultAzureCredentialFactory, DefaultAzureCredentialFactory>();
            builder.Services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            builder.Services.AddSingleton<IGeneratePdfHttpRequestFactory, GeneratePdfHttpRequestFactory>();
            builder.Services.AddSingleton<ITextExtractorHttpRequestFactory, TextExtractorHttpRequestFactory>();
            builder.Services.AddTransient<IHttpRequestFactory, HttpRequestFactory>();
            builder.Services.AddTransient<ICaseDocumentMapper<DdeiCaseDocumentResponse>, DdeiCaseDocumentMapper>();
            
            builder.Services.AddHttpClient<IDdeiDocumentExtractionService, DdeiDocumentExtractionService>(client =>
            {
                client.BaseAddress = new Uri(GetValueFromConfig(configuration, ConfigKeys.SharedKeys.DocumentsRepositoryBaseUrl));
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });
        }
        
        private static string GetValueFromConfig(IConfiguration configuration, string key)
        {
            var value = configuration[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exception($"Value cannot be null: {key}");
            }

            return value;
        }
    }
}