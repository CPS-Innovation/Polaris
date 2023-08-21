using System.Diagnostics.CodeAnalysis;
using System.IO;
using Common.Configuration;
using Common.Domain.Validators;
using Common.Dto.Request;
using Common.Handlers;
using Common.Handlers.Contracts;
using Common.Health;
using Common.Services.DocumentEvaluation;
using Common.Services.DocumentEvaluation.Contracts;
using Common.Services.Extensions;
using Common.Telemetry;
using Common.Telemetry.Contracts;
using FluentValidation;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pdf_generator.Services.DocumentRedactionService;
using pdf_generator.Services.Extensions;

[assembly: FunctionsStartup(typeof(pdf_generator.Startup))]
namespace pdf_generator
{
    [ExcludeFromCodeCoverage]
    internal class Startup : BaseDependencyInjectionStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var services = builder.Services;

            services.AddSingleton<IConfiguration>(Configuration);
            services.AddBlobStorageWithDefaultAzureCredential(Configuration);
            services.AddPdfGenerator();
            services.AddTransient<IDocumentEvaluationService, DocumentEvaluationService>();
            services.AddTransient<IDocumentRedactionService, DocumentRedactionService>();
            services.AddScoped<IValidator<RedactPdfRequestDto>, RedactPdfRequestValidator>();
            services.AddTransient<IExceptionHandler, ExceptionHandler>();
            services.AddSingleton<ITelemetryClient, TelemetryClient>();

            BuildHealthChecks(services);
        }

        /// <summary>
        /// see https://www.davidguida.net/azure-api-management-healthcheck/ for pattern
        /// Microsoft.Extensions.Diagnostics.HealthChecks Nuget downgraded to lower release to get package to work
        /// </summary>
        /// <param name="services"></param>
        private static void BuildHealthChecks(IServiceCollection services)
        {
            services.AddHealthChecks()
                 .AddCheck<AzureSearchClientHealthCheck>("Azure Search Client")
                 .AddCheck<AzureBlobServiceClientHealthCheck>("Azure Blob Service Client");
        }
    }
}