using System.Diagnostics.CodeAnalysis;
using Common.Configuration;
using Common.Domain.Validators;
using Common.Dto.Request;
using Common.Handlers.Contracts;
using Common.Handlers;
using Common.Health;
using Common.Services.DocumentEvaluation.Contracts;
using Common.Services.DocumentEvaluation;
using Common.Services.Extensions;
using Common.Telemetry.Wrappers.Contracts;
using Common.Telemetry.Wrappers;
using FluentValidation;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pdf_generator.Services.DocumentRedactionService;
using pdf_generator.Services.Extensions;
using Common.Telemetry.Contracts;
using Common.Telemetry;
using pdf_generator.Services.DocumentRedactionService.RedactionProvider;
using pdf_generator.Services.DocumentRedactionService.RedactionProvider.ImageConversion;

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
            services.Configure<ImageConversionOptions>(Configuration.GetSection(ImageConversionOptions.ConfigKey));
            services.AddBlobStorageWithDefaultAzureCredential(Configuration);
            services.AddPdfGenerator(Configuration);

            services.AddTransient<IDocumentEvaluationService, DocumentEvaluationService>();
            services.AddTransient<IDocumentRedactionService, DocumentRedactionService>();
            services.AddSingleton(_ => new ImageConversionOptions
            {
                Resolution = 150, //default for Aspsose
                QualityPercent = 33
            });
            services.AddTransient<IRedactionProvider, ImageConversionProvider>();
            services.AddScoped<IValidator<RedactPdfRequestDto>, RedactPdfRequestValidator>();
            services.AddTransient<IExceptionHandler, ExceptionHandler>();
            services.AddSingleton<ITelemetryClient, TelemetryClient>();
            services.AddSingleton<ITelemetryAugmentationWrapper, TelemetryAugmentationWrapper>();
            BuildHealthChecks(services);
        }

        /// <summary>
        /// see https://www.davidguida.net/azure-api-management-healthcheck/ for pattern
        /// Microsoft.Extensions.Diagnostics.HealthChecks Nuget downgraded to lower release to get package to work.
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