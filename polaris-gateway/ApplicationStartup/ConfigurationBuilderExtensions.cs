using Microsoft.Extensions.Configuration;
using System.IO;

namespace PolarisGateway.ApplicationStartup;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddConfigurationSettings(this IConfigurationBuilder builder)
    {
        var configurationBuilder = builder
            .AddEnvironmentVariables()
#if DEBUG
            .SetBasePath(Directory.GetCurrentDirectory())
#endif
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);

        return builder;
    }
}
