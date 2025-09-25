using Common.Domain.Document;
using Common.Wrappers;
using Ddei.Extensions;
using DdeiClient.Factories;
using DdeiClient.Services.DocumentRetrieval;
using Microsoft.Extensions.DependencyInjection;
using pdf_generator.Factories;
using pdf_generator.Factories.Contracts;
using pdf_generator.Models;
using pdf_generator.Services.PdfServices;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace pdf_generator.Services.Extensions;

public static class ServiceCollectionExtension
{
    public static void AddPdfGenerator(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddKeyedPdfService<WordsPdfService>(FileTypesForPdfServicesDto.WordsFileTypes);
        services.AddKeyedPdfService<HtePdfService>(FileTypesForPdfServicesDto.HteFileTypes);
        services.AddKeyedPdfService<CellsPdfService>(FileTypesForPdfServicesDto.CellsFileTypes);
        services.AddKeyedPdfService<SlidesPdfService>(FileTypesForPdfServicesDto.SlidesFileTypes);
        services.AddKeyedPdfService<ImagingPdfService>(FileTypesForPdfServicesDto.ImagingFileTypes);
        services.AddKeyedPdfService<DiagramPdfService>(FileTypesForPdfServicesDto.DiagramFileTypes);
        services.AddKeyedPdfService<EmailPdfService>(FileTypesForPdfServicesDto.EmailFileTypes);
        services.AddKeyedPdfService<PdfRendererService>(FileTypesForPdfServicesDto.PdfRendererFileTypes);
        services.AddKeyedPdfService<XpsPdfRendererService>(FileTypesForPdfServicesDto.XpsPdfRendererFileTypes);
        services.AddKeyedPdfService<DocumentRetrievalWordsPdfService>(FileTypesForPdfServicesDto.DocumentRetrievalFileTypes);
        services.AddSingleton<IPdfService, WordsPdfService>();
        services.AddKeyedSingleton<IPdfService, DocumentTypeUnsupportedPdfService>(null);

        services.AddSingleton<IPdfOrchestratorService, PdfOrchestratorService>();
        services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
        services.AddTransient<IAsposeItemFactory, AsposeItemFactory>();
        services.AddTransient<IPdfServiceFactory, PdfServiceFactory>();

        services.AddSingleton<IDocumentRetrievalServiceFactory, DocumentRetrievalServiceFactory>();
        services.AddKeyedScoped<IDocumentRetrievalService, DocumentRetrievalService>(DocumentNature.Types.Document);
        services.AddKeyedScoped<IDocumentRetrievalService, PreChargeDecisionDocumentRetrievalService>(DocumentNature.Types.PreChargeDecisionRequest);
        services.AddKeyedScoped<IDocumentRetrievalService, DefendantsAndChargesDocumentRetrievalService>(DocumentNature.Types.DefendantsAndCharges);
        services.AddSingleton<IDdeiArgFactory, DdeiArgFactory>();
        services.AddDdeiClientGateway(configuration);
    }

    private static IServiceCollection AddKeyedPdfService<TImplementation>(this IServiceCollection services, List<FileType> fileTypes)
        where TImplementation : class, IPdfService
    {
        foreach (var fileType in fileTypes)
        {
            services.AddKeyedSingleton<IPdfService, TImplementation>(fileType);
        }

        return services;
    }
}