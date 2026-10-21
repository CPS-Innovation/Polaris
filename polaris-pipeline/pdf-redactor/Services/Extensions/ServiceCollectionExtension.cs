// <copyright file="ServiceCollectionExtension.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace pdf_redactor.Services.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pdf_redactor.Services.DocumentManipulation;
using pdf_redactor.Services.DocumentManipulation.Aspose;
using pdf_redactor.Services.DocumentRedaction;
using pdf_redactor.Services.DocumentRedaction.Aspose;
using pdf_redactor.Services.DocumentRedaction.Aspose.RedactionImplementations;

public static class ServiceCollectionExtension
{
    public static void AddRedactionServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDocumentRedactionService, DocumentRedactionService>();
        services.AddSingleton<IDocumentManipulationService, DocumentManipulationService>();
        services.AddSingleton<IRedactionProvider, AsposeRedactionProvider>();
        services.AddSingleton<ICoordinateCalculator, CoordinateCalculator>();
        services.AddSingleton<IRedactionImplementation, DirectImplementation>();
        services.AddSingleton<IRedactionImplementation, ImageConversionImplementation>();
        services.AddSingleton<IDocumentManipulationProvider, AsposeDocumentManipulationProvider>();
        services.Configure<ImageConversionOptions>(configuration.GetSection(ImageConversionOptions.ConfigKey));
    }
}
