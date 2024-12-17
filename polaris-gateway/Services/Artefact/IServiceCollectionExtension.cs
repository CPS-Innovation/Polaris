

using Common.Services.RenderHtmlService;
using Microsoft.Extensions.DependencyInjection;
using PolarisGateway.Services.Artefact.Factories;

namespace PolarisGateway.Services.Artefact
{
    public static class IServiceCollectionExtension
    {
        public static void AddArtefactService(this IServiceCollection services)
        {
            services.AddSingleton<IPdfRetrievalService, PdfRetrievalService>();
            services.AddSingleton<IPdfArtefactService, PdfArtefactService>();
            services.AddSingleton<IPiiArtefactService, PiiArtefactService>();
            services.AddSingleton<IOcrArtefactService, OcrArtefactService>();
            services.AddSingleton<ICacheService, CacheService>();
            services.AddSingleton<IArtefactServiceResponseFactory, ArtefactServiceResponseFactory>();
            services.AddSingleton<IConvertModelToHtmlService, ConvertModelToHtmlService>();
        }
    }
}