using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using coordinator.Durable.Entity;
using Common.Dto.Response.Documents;
using Mapster;
using coordinator.Durable.Payloads.Domain;

namespace coordinator.Mappers
{
    public static class MapsterConfig
    {
        public static void RegisterCoordinatorMapsterConfiguration(this IServiceCollection services)
        {
            TypeAdapterConfig<CaseDurableEntity, TrackerDto>
                .NewConfig()
                .Map
                (
                    dest => dest,
                    src => src
                )
                .Map
                (
                    dest => dest.ProcessingCompleted,
                    src => GetProcessingCompleted(src)
                )
                .Map
                (
                    dest => dest.Documents,
                    src => Enumerable.Empty<TrackerDocumentDto>()
                        .Concat(src.CmsDocuments.Adapt<TrackerDocumentDto[]>())
                        .Concat(src.PcdRequests.Adapt<TrackerDocumentDto[]>())
                        .Concat(
                            (src.DefendantsAndCharges != null && src.DefendantsAndCharges.HasMultipleDefendants
                                    ? new[] { src.DefendantsAndCharges }
                                    : Enumerable.Empty<DefendantsAndChargesEntity>())
                            .Adapt<TrackerDocumentDto[]>()
                        )
                );
        }

        private static DateTime? GetProcessingCompleted(CaseDurableEntity caseEntity) =>
            caseEntity.Completed.HasValue
                ? caseEntity.Running.Value.AddSeconds(caseEntity.Completed.Value).ToUniversalTime()
                : null;
    }
}
