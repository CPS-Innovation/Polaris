using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Common.Configuration;
using Common.Services.Extensions;
using Common.Wrappers;
using coordinator;
using coordinator.Factories;
using coordinator.Clients;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Common.Wrappers.Contracts;
using FluentValidation;
using Common.Domain.Validators;
using Common.Dto.Request;
using Ddei.Services.Extensions;
using Common.Handlers.Contracts;
using Common.Handlers;
using coordinator.Constants;
using coordinator.Services.RenderHtmlService;
using coordinator.Mappers;
using Common.Telemetry.Contracts;
using Common.Telemetry;
using coordinator.Durable.Providers;
using coordinator.Validators;
using coordinator.Services.DocumentToggle;
using Common.Streaming;
using coordinator.Services.TextExtractService;
using coordinator.Services.CleardownService;
using coordinator.Durable.Payloads;
using coordinator.Functions.DurableEntity.Entity.Mapper;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

[assembly: FunctionsStartup(typeof(Startup))]
namespace coordinator
{
    [ExcludeFromCodeCoverage]
    internal class Startup : BaseDependencyInjectionStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var services = builder.Services;

            services.AddSingleton<IConfiguration>(Configuration);
            services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            services.AddTransient<IValidatorWrapper<CaseDocumentOrchestrationPayload>, ValidatorWrapper<CaseDocumentOrchestrationPayload>>();
            services.AddSingleton<IConvertModelToHtmlService, ConvertModelToHtmlService>();
            services.AddTransient<IPipelineClientRequestFactory, PipelineClientRequestFactory>();
            services.AddTransient<ITextExtractorClientRequestFactory, TextExtractorClientRequestFactory>();
            services.AddTransient<IQueryConditionFactory, QueryConditionFactory>();
            services.AddTransient<IExceptionHandler, ExceptionHandler>();
            services.AddSingleton<IHttpResponseMessageStreamFactory, HttpResponseMessageStreamFactory>();
            services.AddBlobStorageWithDefaultAzureCredential(Configuration);

            services.AddHttpClient<IPdfGeneratorClient, PdfGeneratorClient>(client =>
            {
                client.BaseAddress = new Uri(Configuration.GetValueFromConfig(ConfigKeys.PipelineRedactPdfBaseUrl));
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });
            services.AddHttpClient<ITextExtractorClient, TextExtractorClient>(client =>
            {
                client.BaseAddress = new Uri(Configuration.GetValueFromConfig(ConfigKeys.PipelineTextExtractorBaseUrl));
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });

            services.AddTransient<ITextExtractService, TextExtractService>();
            services.AddTransient<ISearchFilterDocumentMapper, SearchFilterDocumentMapper>();
            services.AddTransient<ITextExtractorClientRequestFactory, TextExtractorClientRequestFactory>();
            services.AddScoped<IValidator<RedactPdfRequestDto>, RedactPdfRequestValidator>();
            services.AddSingleton<ICmsDocumentsResponseValidator, CmsDocumentsResponseValidator>();
            services.AddSingleton<ICleardownService, CleardownService>();
            services.AddTransient<IOrchestrationProvider, OrchestrationProvider>();

            services.RegisterMapsterConfiguration();
            services.AddDdeiClient(Configuration);
            services.AddTransient<IDocumentToggleService, DocumentToggleService>();
            services.AddSingleton<IDocumentToggleService>(new DocumentToggleService(
              DocumentToggleService.ReadConfig()
            ));

            services.AddSingleton<ITelemetryClient, TelemetryClient>();
            services.AddSingleton<ICaseDurableEntityMapper, CaseDurableEntityMapper>();

            services.AddDurableClientFactory();
        }
    }
}