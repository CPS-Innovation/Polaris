using System.Diagnostics.CodeAnalysis;
using Common.Wrappers;
using Common.Wrappers.Contracts;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ddei.Services.Extensions;
using Common.Configuration;

[assembly: FunctionsStartup(typeof(PolarisAuthHandover.Startup))]

namespace PolarisAuthHandover
{
    [ExcludeFromCodeCoverage]
    internal class Startup : BaseDependencyInjectionStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var services = builder.Services;

            services.AddSingleton<IConfiguration>(Configuration);
            services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();

            services.AddDdeiClient(Configuration);
        }
    }
}
