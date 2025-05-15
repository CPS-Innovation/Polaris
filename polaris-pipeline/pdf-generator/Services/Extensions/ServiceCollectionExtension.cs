﻿using Common.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using pdf_generator.Factories;
using pdf_generator.Factories.Contracts;
using pdf_generator.Services.PdfService;
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
                    diagramPdfService, emailPdfService, pdfRendererService, xpsPdfRendererService, loggingService!);
            });
            services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            services.AddTransient<IAsposeItemFactory, AsposeItemFactory>();
        }
    }
}