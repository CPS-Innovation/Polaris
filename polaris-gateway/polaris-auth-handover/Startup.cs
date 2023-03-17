using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Common.Wrappers;
using Common.Wrappers.Contracts;
using Ddei.Clients;
using Ddei.Factories;
using Ddei.Factories.Contracts;
using Ddei.Options;
using Ddei.Services;
using Ddei.Services.Contract;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PolarisGateway.CaseDataImplementations.Ddei.Mappers;
using PolarisGateway.CaseDataImplementations.Ddei.Services;
using PolarisGateway.Services;

[assembly: FunctionsStartup(typeof(PolarisAuthHandover.Startup))]

namespace PolarisAuthHandover
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
            builder.Services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            builder.Services.AddTransient<ICmsAuthValuesFactory, CmsAuthValuesFactory>();

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
    }
}
