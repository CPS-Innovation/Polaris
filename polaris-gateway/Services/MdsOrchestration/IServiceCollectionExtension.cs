using Microsoft.Extensions.DependencyInjection;
using PolarisGateway.Services.MdsOrchestration.Mappers;

namespace PolarisGateway.Services.MdsOrchestration
{
    public static class IServiceCollectionExtension
    {
        public static void AddMdsOrchestrationService(this IServiceCollection services)
        {
            services.AddSingleton<IMdsCaseDocumentsOrchestrationService, MdsCaseDocumentsOrchestrationService>();
            services.AddSingleton<IMdsReclassifyDocumentOrchestrationService, MdsReclassifyDocumentOrchestrationService>();
            services.AddSingleton<IMdsCaseOrchestrationService, MdsCaseOrchestrationService>();
            services.AddSingleton<IMdsCaseOrchestrationService, MdsCaseOrchestrationService>();
            services.AddSingleton<IDocumentDtoMapper, DocumentDtoMapper>();
        }
    }
}