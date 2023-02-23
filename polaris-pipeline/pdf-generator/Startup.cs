using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using Azure.Identity;
using Azure.Storage.Blobs;
using Common.Constants;
using Common.Domain.Requests;
using Common.Domain.Responses;
using Common.Exceptions.Contracts;
using Common.Factories;
using Common.Factories.Contracts;
using Common.Health;
using Common.Mappers;
using Common.Mappers.Contracts;
using Common.Services.BlobStorageService;
using Common.Services.BlobStorageService.Contracts;
using Common.Services.DocumentEvaluation;
using Common.Services.DocumentEvaluation.Contracts;
using Common.Services.DocumentExtractionService;
using Common.Services.DocumentExtractionService.Contracts;
using Common.Wrappers;
using FluentValidation;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using pdf_generator.Domain.Validators;
using pdf_generator.Factories;
using pdf_generator.Handlers;
using pdf_generator.Services.DocumentRedactionService;
using pdf_generator.Services.PdfService;

[assembly: FunctionsStartup(typeof(pdf_generator.Startup))]
namespace pdf_generator
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
            
            builder.Services.AddSingleton<IPdfService, WordsPdfService>();
            builder.Services.AddSingleton<IPdfService, CellsPdfService>();
            builder.Services.AddSingleton<IPdfService, SlidesPdfService>();
            builder.Services.AddSingleton<IPdfService, ImagingPdfService>();
            builder.Services.AddSingleton<IPdfService, DiagramPdfService>();
            builder.Services.AddSingleton<IPdfService, HtmlPdfService>();
            builder.Services.AddSingleton<IPdfService, EmailPdfService>();
            builder.Services.AddSingleton<IPdfService, PdfRendererService>();
            builder.Services.AddSingleton<IPdfOrchestratorService, PdfOrchestratorService>(provider =>
            {
                var pdfServices = provider.GetServices<IPdfService>();
                var servicesList = pdfServices.ToList();
                var wordsPdfService = servicesList.First(s => s.GetType() == typeof(WordsPdfService));
                var cellsPdfService = servicesList.First(s => s.GetType() == typeof(CellsPdfService));
                var slidesPdfService = servicesList.First(s => s.GetType() == typeof(SlidesPdfService));
                var imagingPdfService = servicesList.First(s => s.GetType() == typeof(ImagingPdfService));
                var diagramPdfService = servicesList.First(s => s.GetType() == typeof(DiagramPdfService));
                var htmlPdfService = servicesList.First(s => s.GetType() == typeof(HtmlPdfService));
                var emailPdfService = servicesList.First(s => s.GetType() == typeof(EmailPdfService));
                var pdfRendererService = servicesList.First(s => s.GetType() == typeof(PdfRendererService));
                var loggingService = provider.GetService<ILogger<PdfOrchestratorService>>();

                return new PdfOrchestratorService(wordsPdfService, cellsPdfService, slidesPdfService, imagingPdfService, 
                    diagramPdfService, htmlPdfService, emailPdfService, pdfRendererService, loggingService);
            });

            builder.Services.AddTransient<ICoordinateCalculator, CoordinateCalculator>();
            builder.Services.AddTransient<IValidatorWrapper<GeneratePdfRequest>, ValidatorWrapper<GeneratePdfRequest>>();
            builder.Services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            builder.Services.AddTransient<IExceptionHandler, ExceptionHandler>();
            builder.Services.AddTransient<IAsposeItemFactory, AsposeItemFactory>();

            builder.Services.AddAzureClients(azureClientFactoryBuilder =>
            {
#if DEBUG 
                if(configuration.IsSettingEnabled(DebugSettings.UseAzureStorageEmulatorFlag))
                {
                    azureClientFactoryBuilder.AddBlobServiceClient(configuration[ConfigKeys.SharedKeys.BlobServiceConnectionString])
                        .WithCredential(new DefaultAzureCredential());
                }
                else
                {
                    azureClientFactoryBuilder.AddBlobServiceClient(new Uri(configuration[ConfigKeys.SharedKeys.BlobServiceUrl]))
                        .WithCredential(new DefaultAzureCredential());
                }
#else
                azureClientFactoryBuilder.AddBlobServiceClient(new Uri(configuration[ConfigKeys.SharedKeys.BlobServiceUrl]))
                    .WithCredential(new DefaultAzureCredential());
#endif
            });
            builder.Services.AddTransient<IBlobStorageService>(serviceProvider =>
            {
                var loggingService = serviceProvider.GetService<ILogger<BlobStorageService>>();
                
                return new BlobStorageService(serviceProvider.GetRequiredService<BlobServiceClient>(),
                        configuration[ConfigKeys.SharedKeys.BlobServiceContainerName], loggingService);
            });

            builder.Services.AddTransient<IHttpRequestFactory, HttpRequestFactory>();
            builder.Services.AddTransient<ICaseDocumentMapper<DdeiCaseDocumentResponse>, DdeiCaseDocumentMapper>();
            
            builder.Services.AddHttpClient<IDdeiDocumentExtractionService, DdeiDocumentExtractionService>(client =>
            {
                client.BaseAddress = new Uri(configuration[ConfigKeys.SharedKeys.DocumentsRepositoryBaseUrl]);
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });

            builder.Services.AddTransient<IDocumentEvaluationService, DocumentEvaluationService>();
            builder.Services.AddTransient<IDocumentRedactionService, DocumentRedactionService>();
            builder.Services.AddScoped<IValidator<RedactPdfRequest>, RedactPdfRequestValidator>();
            builder.Services.AddTransient<ISearchClientFactory, SearchClientFactory>();

            BuildHealthChecks(builder);
        }

        /// <summary>
        /// see https://www.davidguida.net/azure-api-management-healthcheck/ for pattern
        /// Microsoft.Extensions.Diagnostics.HealthChecks Nuget downgraded to lower release to get package to work
        /// </summary>
        /// <param name="builder"></param>
        private static void BuildHealthChecks(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHealthChecks()
                 .AddCheck<AzureSearchClientHealthCheck>("Azure Search Client")
                 .AddCheck<AzureBlobServiceClientHealthCheck>("Azure Blob Service Client")
                 .AddCheck<DdeiDocumentExtractionServiceHealthCheck>("DDEI Document Extraction Service");
        }
    }
}