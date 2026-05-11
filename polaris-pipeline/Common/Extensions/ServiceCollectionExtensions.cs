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
            // The Application Insights SDK adds a default logging filter that instructs ILogger to capture only Warning and more severe logs. Application Insights requires an explicit override.
            // Log levels can also be configured using appsettings.json. For more information, see https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service#ilogger-logs
            var toRemove = options.Rules
                .FirstOrDefault(rule => string.Equals(rule.ProviderName, typeof(ApplicationInsightsLoggerProvider).FullName));

            if (toRemove is not null)
            {
                options.Rules.Remove(toRemove);
            }

            // Add explicit rules to allow logs through to Application Insights in .NET 8 Isolated mode
            // Without these rules, ILogger messages won't reach App Insights even after removing the restrictive filter

            // Capture all application logs at Information level and above (includes Warning, Error, Critical)
            options.Rules.Add(new LoggerFilterRule(
                typeof(ApplicationInsightsLoggerProvider).FullName,
                null, // All categories
                LogLevel.Information,
                null));

            // Reduce noise from Microsoft internal libraries - only capture warnings and above
            options.Rules.Add(new LoggerFilterRule(
                typeof(ApplicationInsightsLoggerProvider).FullName,
                "Microsoft",
                LogLevel.Warning,
                null));

            // Ensure host logs are captured at Information level
            options.Rules.Add(new LoggerFilterRule(
                typeof(ApplicationInsightsLoggerProvider).FullName,
                "Host",
                LogLevel.Information,
                null));

            // Ensure function execution logs are captured at Information level
            options.Rules.Add(new LoggerFilterRule(
                typeof(ApplicationInsightsLoggerProvider).FullName,
                "Function",
                LogLevel.Information,
                null));
        });
}