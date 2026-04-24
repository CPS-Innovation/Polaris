// <copyright file="Program.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

using Common.Extensions;
using Common.Middleware;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using PolarisGateway.ApplicationStartup;
using PolarisGateway.Middleware;
using System;
using System.Linq;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(options =>
    {
        options.UseMiddleware<ExceptionHandlingMiddleware>();
        options.UseWhen<RequestValidationMiddleware>(context =>
        {
            // Apply middleware only for non-swagger endpoints
            var path = context.GetHttpContext()?.Request?.Path.Value;

            if (string.IsNullOrEmpty(path))
                return false;

            return !path.Contains("swagger", StringComparison.OrdinalIgnoreCase)
                   && !path.Contains("openapi", StringComparison.OrdinalIgnoreCase);
        });
        options.UseWhen<SecureHttpHeadersMiddleware>(context =>
        {
            return context.FunctionDefinition.InputBindings
            .Any(binding => binding.Value.Type == "httpTrigger");
        });
    })
    .ConfigureOpenApi()
    .ConfigureLogging(options => options.AddApplicationInsights())
    .ConfigureAppConfiguration(builder => builder.AddConfigurationSettings())
    .ConfigureServices((services) =>
    {
#if DEBUG
        // https://stackoverflow.com/questions/54435551/invalidoperationexception-idx20803-unable-to-obtain-configuration-from-pii
        IdentityModelEventSource.ShowPII = true;
        IdentityModelEventSource.LogCompleteSecurityArtifact = true;
#endif
        services.ConfigureServices();
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        // Commented out to match WMA instance of DDEI configuration and see if it improves log visibility
        /* services.Configure<TelemetryConfiguration>(telemetryConfiguration =>
        {
            telemetryConfiguration.DefaultTelemetrySink.TelemetryProcessorChainBuilder
                .UseAdaptiveSampling(maxTelemetryItemsPerSecond: 20, excludedTypes: "Request;Exception;Event;Trace");
            telemetryConfiguration.DisableTelemetry = false;
        }); */
        services.ConfigureLoggerFilterOptions();

        // Remove server header to satisfy ITHC requirement.
        services.Configure<KestrelServerOptions>(k => k.AddServerHeader = false);
    })
    .Build();


ILogger<Program> logger = host.Services.GetRequiredService<ILogger<Program>>();
StartupHelpers.SetAsposeLicence(logger);

host.Run();

namespace ApplicationStartup
{
    public partial class Program;
}