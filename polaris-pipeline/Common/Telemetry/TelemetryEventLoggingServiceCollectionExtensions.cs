// <copyright file="TelemetryEventLoggingServiceCollectionExtensions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

 using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AppInsights = Microsoft.ApplicationInsights;

namespace Common.Telemetry
{
    public static class TelemetryEventLoggingServiceCollectionExtensions
    {
        /// <summary>
        /// Registers an <see cref="ILoggerProvider"/> that converts business event telemetry raised via
        /// ILogger.TrackEvent/TrackEventFailure (see <see cref="TelemetryEventLoggerExtensions"/>) into Application
        /// Insights customEvents, preserving the shape previously produced by the (now removed) ITelemetryClient.
        /// Requires Microsoft.ApplicationInsights.TelemetryClient to already be registered in the service collection
        /// (e.g. via AddApplicationInsightsTelemetryWorkerService).
        /// </summary>
        public static IServiceCollection AddTelemetryEventLogging(this IServiceCollection services) =>
            services.AddSingleton<ILoggerProvider>(sp =>
                new TelemetryEventLoggerProvider(sp.GetRequiredService<AppInsights.TelemetryClient>()));
    }
}
