using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Common.Dto.Response.Documents;
using Mapster;
using coordinator.Domain;
using coordinator.Durable.Payloads.Domain;

namespace coordinator.Mappers
{
    public static class MapsterConfig
    {
        public static void RegisterCoordinatorMapsterConfiguration(this IServiceCollection services)
        {
            TypeAdapterConfig<(CaseDurableEntityState, CaseDurableEntityDocumentsState), TrackerDto>
                .NewConfig()
                .Map
                (
                    dest => dest,
                    src => src.Item1
                )
                .Map
                (
                    dest => dest.DocumentsRetrieved,
                    src => GetDocumentsRetrieved(src.Item1)
                )
                .Map
                (
                    dest => dest.ProcessingCompleted,
                    src => GetProcessingCompleted(src.Item1)
                )
                .Map
                (
                    dest => dest.Documents,
                    src => Enumerable.Empty<DocumentDto>()
                        .Concat(src.Item2.CmsDocuments.Adapt<DocumentDto[]>())
                        .Concat(src.Item2.PcdRequests.Adapt<DocumentDto[]>())
                        .Concat(
                            (src.Item2.DefendantsAndCharges != null && src.Item2.DefendantsAndCharges.HasMultipleDefendants
                                    ? new[] { src.Item2.DefendantsAndCharges }
                                    : Enumerable.Empty<DefendantsAndChargesEntity>())
                            .Adapt<DocumentDto[]>()
                        )
                );
        }

        private static DateTime? GetDocumentsRetrieved(CaseDurableEntityState caseEntity) =>
            caseEntity.Retrieved.HasValue && caseEntity.Running.HasValue
                ? caseEntity.Running.Value.AddSeconds(caseEntity.Retrieved.Value).ToUniversalTime()
                : null;


        private static DateTime? GetProcessingCompleted(CaseDurableEntityState caseEntity) =>
            caseEntity.Retrieved.HasValue && caseEntity.Completed.HasValue
                ? caseEntity.Running.Value.AddSeconds(caseEntity.Completed.Value).ToUniversalTime()
                : null;
    }
}
