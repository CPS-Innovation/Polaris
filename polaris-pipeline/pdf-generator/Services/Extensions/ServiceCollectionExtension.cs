using Common.Wrappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using pdf_generator.Factories;
using pdf_generator.Factories.Contracts;
using pdf_generator.Services.DocumentRedaction;
using pdf_generator.Services.DocumentRedaction.Aspose;
using pdf_generator.Services.DocumentRedaction.Aspose.RedactionImplementations;
using pdf_generator.Services.PdfService;
using pdf_generator.Services.SyncFusionPdfService;
using System.Linq;

namespace pdf_generator.Services.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddPdfGenerator(this IServiceCollection services)
        {
            services.AddSingleton<IPdfService, WordsPdfService>();
            services.AddSingleton<IPdfService, CellsPdfService>();
            services.AddSingleton<IPdfService, SlidesPdfService>();
            services.AddSingleton<IPdfService, ImagingPdfService>();
            services.AddSingleton<IPdfService, DiagramPdfService>();
            services.AddSingleton<IPdfService, EmailPdfService>();
            services.AddSingleton<IPdfService, PdfRendererService>();
            services.AddSingleton<IPdfService, XpsPdfRendererService>();
            services.AddSingleton<IPdfOrchestratorService, PdfOrchestratorService>(provider =>
            {
                var pdfServices = provider.GetServices<IPdfService>();
                var servicesList = pdfServices.ToList();
                var wordsPdfService = servicesList.First(s => s.GetType() == typeof(WordsPdfService));
                var cellsPdfService = servicesList.First(s => s.GetType() == typeof(CellsPdfService));
                var slidesPdfService = servicesList.First(s => s.GetType() == typeof(SlidesPdfService));
                var imagingPdfService = servicesList.First(s => s.GetType() == typeof(ImagingPdfService));
                var diagramPdfService = servicesList.First(s => s.GetType() == typeof(DiagramPdfService));
                var emailPdfService = servicesList.First(s => s.GetType() == typeof(EmailPdfService));
                var pdfRendererService = servicesList.First(s => s.GetType() == typeof(PdfRendererService));
                var xpsPdfRendererService = servicesList.First(s => s.GetType() == typeof(XpsPdfRendererService));
                var loggingService = provider.GetService<ILogger<PdfOrchestratorService>>();

                return new PdfOrchestratorService(wordsPdfService, cellsPdfService, slidesPdfService, imagingPdfService,
                    diagramPdfService, emailPdfService, pdfRendererService, xpsPdfRendererService, loggingService);
            });
            services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            services.AddTransient<IAsposeItemFactory, AsposeItemFactory>();
        }

        public static void AddSyncFusionPdfGenerator(this IServiceCollection services)
        {
            services.AddSingleton<ISyncFusionPdfService, SyncFusionWordsPdfService>();
            services.AddSingleton<ISyncFusionPdfService, SyncFusionCellsPdfService>();
            services.AddSingleton<ISyncFusionPdfService, SyncFusionSlidesPdfService>();
            services.AddSingleton<ISyncFusionPdfService, SyncFusionImagingPdfService>();
            services.AddSingleton<ISyncFusionPdfService, SyncFusionXpsPdfService>();
            services.AddSingleton<ISyncFusionPdfService, SyncFusionHtmlPdfService>();
            services.AddSingleton<ISyncFusionPdfService, SyncFusionPdfRendererService>();

            services.AddSingleton<ISyncFusionPdfOrchestratorService, SyncFusionPdfOrchestratorService>(provider =>
            {
                var syncFusionPdfServices = provider.GetServices<ISyncFusionPdfService>();
                var servicesList = syncFusionPdfServices.ToList();
                var wordsPdfService = servicesList.First(s => s.GetType() == typeof(SyncFusionWordsPdfService));
                var cellsPdfService = servicesList.First(s => s.GetType() == typeof(SyncFusionCellsPdfService));
                var slidesPdfService = servicesList.First(s => s.GetType() == typeof(SyncFusionSlidesPdfService));
                var imagePdfService = servicesList.First(s => s.GetType() == typeof(SyncFusionImagingPdfService));
                var xpsPdfRendererService = servicesList.First(s => s.GetType() == typeof(SyncFusionXpsPdfService));
                var htmlPdfService = servicesList.First(s => s.GetType() == typeof(SyncFusionHtmlPdfService));
                var pdfService = servicesList.First(s => s.GetType() == typeof(SyncFusionPdfRendererService));

                return new SyncFusionPdfOrchestratorService(wordsPdfService, cellsPdfService, slidesPdfService, imagePdfService, xpsPdfRendererService, htmlPdfService, pdfService);
            });
            services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            services.AddTransient<ISyncFusionItemFactory, SyncFusionItemFactory>();
        }

        public static void AddRedactionServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IUploadFileNameFactory, UploadFileNameFactory>();
            services.AddSingleton<IDocumentRedactionService, DocumentRedactionService>();
            // Aspose-specific services
            services.AddSingleton<IRedactionProvider, AsposeRedactionProvider>();
            services.AddSingleton<ICoordinateCalculator, CoordinateCalculator>();
            services.AddSingleton<IRedactionImplementation, ImageConversionImplementation>();
            services.Configure<ImageConversionOptions>(configuration.GetSection(ImageConversionOptions.ConfigKey));
        }
    }
}