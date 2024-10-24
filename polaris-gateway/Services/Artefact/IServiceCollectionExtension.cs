

using Common.Services.RenderHtmlService;
using Microsoft.Extensions.DependencyInjection;
using PolarisGateway.Services.Artefact.Factories;

namespace PolarisGateway.Services.Artefact
{
    public static class IServiceCollectionExtension
    {
        public static void AddArtefactService(this IServiceCollection services)
        {
            services.AddSingleton<IArtefactService, ArtefactService>();
            services.AddSingleton<ICachingArtefactService, CachingArtefactService>();
            services.AddSingleton<IArtefactServiceResponseFactory, ArtefactServiceResponseFactory>();
            services.AddSingleton<IConvertModelToHtmlService, ConvertModelToHtmlService>();
        }
    }
}