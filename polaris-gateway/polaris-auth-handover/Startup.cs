using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Common.Wrappers;
using Common.Wrappers.Contracts;
using Ddei.Services;
using Ddei.Factories;
using Ddei.Factories.Contracts;
using Ddei.Mappers;
using Ddei.Options;
using DdeiClient.Services.Contracts;
using DdeiClient.Mappers.Contract;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ddei.Services.Extensions;

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

            builder.Services.AddDdeiClient(configuration);
        }
    }
}
