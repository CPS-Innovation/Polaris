using Common.Domain.Document;
using Microsoft.Extensions.DependencyInjection;
using pdf_generator.Services.PdfServices;
using System;

namespace pdf_generator.Factories;

public class PdfServiceFactory : IPdfServiceFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PdfServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IPdfService Create(FileType fileType)
    {
        return _serviceProvider.GetKeyedService<IPdfService>(fileType) ??
               _serviceProvider.GetKeyedService<IPdfService>(null)!;
    }
}