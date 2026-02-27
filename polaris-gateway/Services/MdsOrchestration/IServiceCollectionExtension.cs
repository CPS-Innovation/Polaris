using Microsoft.Extensions.DependencyInjection;
using PolarisGateway.Services.DdeiOrchestration.Mappers;

namespace PolarisGateway.Services.DdeiOrchestration
{
    public static class IServiceCollectionExtension
    {
        public static void AddDdeiOrchestrationService(this IServiceCollection services)
        {
            services.AddSingleton<IMdsCaseDocumentsOrchestrationService, MdsCaseDocumentsOrchestrationService>();
            services.AddSingleton<IMdsReclassifyDocumentOrchestrationService, MdsReclassifyDocumentOrchestrationService>();
            services.AddSingleton<IMdsCaseOrchestrationService, MdsCaseOrchestrationService>();
            services.AddSingleton<IMdsCaseOrchestrationService, MdsCaseOrchestrationService>();
            services.AddSingleton<IDocumentDtoMapper, DocumentDtoMapper>();
        }
    }
}