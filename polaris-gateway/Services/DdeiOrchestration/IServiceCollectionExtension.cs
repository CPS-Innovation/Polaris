


using Microsoft.Extensions.DependencyInjection;
using PolarisGateway.Services.DdeiOrchestration.Mappers;

namespace PolarisGateway.Services.DdeiOrchestration
{
    public static class IServiceCollectionExtension
    {
        public static void AddDdeiOrchestrationService(this IServiceCollection services)
        {
            services.AddSingleton<IDdeiCaseDocumentsOrchestrationService, DdeiCaseDocumentsOrchestrationService>();
            services.AddSingleton<IDdeiReclassifyDocumentOrchestrationService, DdeiReclassifyDocumentOrchestrationService>();
            services.AddSingleton<IDocumentDtoMapper, DocumentDtoMapper>();
        }
    }
}