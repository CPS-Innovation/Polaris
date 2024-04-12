using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pdf_redactor.Factories.AsposeRedactionImplementationFactory;
using pdf_redactor.Factories.SyncFusionRedactionImplementationFactory;
using pdf_redactor.Factories.RedactionProviderFactory;
using pdf_redactor.Services.DocumentRedaction;
using pdf_redactor.Services.DocumentRedaction.Aspose;
using SyncFusionRedactionImplementations = pdf_redactor.Services.DocumentRedaction.SyncFusion.RedactionImplementations;
using AsposeRedactionImplementations = pdf_redactor.Services.DocumentRedaction.Aspose.RedactionImplementations;
using pdf_redactor.Services.DocumentRedaction.SyncFusion;

namespace pdf_redactor.Services.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddRedactionServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IUploadFileNameFactory, UploadFileNameFactory>();
            services.AddSingleton<IDocumentRedactionService, DocumentRedactionService>();
            // Aspose-specific services
            services.AddSingleton<AsposeRedactionProvider>();
            services.AddSingleton<SyncFusionRedactionProvider>();
            services.AddSingleton<IRedactionProviderFactory, RedactionProviderFactory>();
            services.AddSingleton<ICoordinateCalculator, CoordinateCalculator>();
            services.AddSingleton<AsposeRedactionImplementations.DirectImplementation>();
            services.AddSingleton<AsposeRedactionImplementations.ImageConversionImplementation>();
            services.AddSingleton<SyncFusionRedactionImplementations.DirectImplementation>();
            services.AddSingleton<SyncFusionRedactionImplementations.ImageConversionImplementation>();
            services.AddSingleton<IAsposeRedactionImplementationFactory, AsposeRedactionImplementationFactory>();
            services.AddSingleton<ISyncFusionRedactionImplementationFactory, SyncFusionRedactionImplementationFactory>();
            services.Configure<AsposeRedactionImplementations.ImageConversionOptions>(configuration.GetSection(AsposeRedactionImplementations.ImageConversionOptions.ConfigKey));
        }
    }
}