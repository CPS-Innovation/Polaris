using Azure.Identity;
using Azure.Search.Documents;
using Azure.Storage.Blobs;
using Common.Clients;
using Common.Clients.Contracts;
using Common.Configuration;
using Common.Constants;
using Common.Dto.Request;
using Common.Exceptions.Contracts;
using Common.Factories;
using Common.Factories.Contracts;
using Common.Mappers;
using Common.Mappers.Contracts;
using Common.Services.SasGeneratorService;
using Common.Wrappers;
using Common.Wrappers.Contracts;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using pdf_generator.Factories;
using pdf_generator.Services.DocumentRedactionService;
using pdf_generator.Services.PdfService;
using System;
using System.Linq;
using text_extractor.Handlers;

namespace Common.Services.Extensions
{
    public static class IServiceCollectionExtension
    {
        public static void AddBlobStorageWithDefaultAzureCredential(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddAzureClients(azureClientFactoryBuilder =>
            {
                var blobServiceUrl = configuration.GetValueFromConfig(ConfigKeys.SharedKeys.BlobServiceUrl);
                azureClientFactoryBuilder.AddBlobServiceClient(new Uri(blobServiceUrl))
                    .WithCredential(new DefaultAzureCredential());
            });

            services.AddTransient((Func<IServiceProvider, IPolarisStorageClient>)(serviceProvider =>
            {
                var logger = serviceProvider.GetService<ILogger<PolarisStorageClient>>();
                BlobServiceClient blobServiceClient = serviceProvider.GetRequiredService<BlobServiceClient>();
                string blobServiceContainerName = configuration.GetValueFromConfig(ConfigKeys.SharedKeys.BlobServiceContainerName);
                var blobStorageClient = new PolarisStorageClient(blobServiceClient, blobServiceContainerName, logger);

                return blobStorageClient;
            }));
        }

        public static void AddBlobStorageWithConnectionString(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddAzureClients(azureClientFactoryBuilder =>
            {
                string blobServiceConnectionString = configuration[ConfigKeys.SharedKeys.BlobServiceConnectionString];
                azureClientFactoryBuilder.AddBlobServiceClient(blobServiceConnectionString);
            });

            services.AddTransient((Func<IServiceProvider, IPolarisStorageClient>)(serviceProvider =>
            {
                var logger = serviceProvider.GetService<ILogger<PolarisStorageClient>>();
                BlobServiceClient blobServiceClient = serviceProvider.GetRequiredService<BlobServiceClient>();
                string blobServiceContainerName = configuration.GetValueFromConfig(ConfigKeys.SharedKeys.BlobServiceContainerName);
                return new PolarisStorageClient(blobServiceClient, blobServiceContainerName, logger);
            }));
        }


        public static void AddBlobSasGenerator(this IServiceCollection services)
        {
            services.AddTransient<IBlobSasBuilderWrapper, BlobSasBuilderWrapper>();
            services.AddTransient<IBlobSasBuilderFactory, BlobSasBuilderFactory>();
            services.AddTransient<IBlobSasBuilderWrapperFactory, BlobSasBuilderWrapperFactory>();
            services.AddTransient<ISasGeneratorService, SasGeneratorService.SasGeneratorService>();
        }

        public static void AddSearchClient(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddOptions<SearchClientOptions>().Configure<IConfiguration>((settings, _) =>
            {
                configuration.GetSection("searchClient").Bind(settings);
            });

            services.AddTransient<ISearchIndexClient, SearchIndexClient>();
            services.AddTransient<ISearchClientFactory, SearchClientFactory>();
            services.AddTransient<IStreamlinedSearchResultFactory, StreamlinedSearchResultFactory>();
            services.AddTransient<IStreamlinedSearchLineMapper, StreamlinedSearchLineMapper>();
            services.AddTransient<IStreamlinedSearchWordMapper, StreamlinedSearchWordMapper>();
        }

        public static void AddPdfGenerator(this IServiceCollection services)
        {
            services.AddSingleton<IPdfService, WordsPdfService>();
            services.AddSingleton<IPdfService, CellsPdfService>();
            services.AddSingleton<IPdfService, SlidesPdfService>();
            services.AddSingleton<IPdfService, ImagingPdfService>();
            services.AddSingleton<IPdfService, DiagramPdfService>();
            services.AddSingleton<IPdfService, HtmlPdfService>();
            services.AddSingleton<IPdfService, EmailPdfService>();
            services.AddSingleton<IPdfService, PdfRendererService>();
            services.AddSingleton<IPdfOrchestratorService, PdfOrchestratorService>(provider =>
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

            services.AddTransient<ICoordinateCalculator, CoordinateCalculator>();
            services.AddTransient<IValidatorWrapper<GeneratePdfRequestDto>, ValidatorWrapper<GeneratePdfRequestDto>>();
            services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            services.AddTransient<IExceptionHandler, ExceptionHandler>();
            services.AddTransient<IAsposeItemFactory, AsposeItemFactory>();
        }
    }
}