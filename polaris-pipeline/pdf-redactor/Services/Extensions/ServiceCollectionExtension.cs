using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pdf_redactor.Services.DocumentRedaction;
using pdf_redactor.Services.DocumentRedaction.Aspose;
using pdf_redactor.Services.DocumentRedaction.Aspose.RedactionImplementations;

namespace pdf_redactor.Services.Extensions
{
    public static class ServiceCollectionExtension
    {
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