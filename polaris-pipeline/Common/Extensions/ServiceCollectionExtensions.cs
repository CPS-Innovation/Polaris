using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using System.Linq;

namespace Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureLoggerFilterOptions(this IServiceCollection services) =>
        services.Configure<LoggerFilterOptions>(options =>
        {
            // See: https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide?tabs=windows#managing-log-levels
            // The Application Insights SDK adds a default logging filter that instructs ILogger to capture only Warning and more severe logs.
            // This filter must be removed to allow host.json log level configuration to take effect in .NET 8 Isolated mode.
            // Log levels are configured in host.json under logging.applicationInsights.logLevel
            var toRemove = options.Rules
                .FirstOrDefault(rule => string.Equals(rule.ProviderName, typeof(ApplicationInsightsLoggerProvider).FullName));

            if (toRemove is not null)
            {
                options.Rules.Remove(toRemove);
            }
        });
}