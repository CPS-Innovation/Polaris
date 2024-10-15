using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Azure.AI.TextAnalytics;
using coordinator.Domain;
using coordinator.Durable.Entity;
using Common.Dto.Tracker;
using Mapster;
using coordinator.Durable.Payloads.Domain;

namespace coordinator.Mappers
{
    public static class MapsterConfig
    {
        public static void RegisterMapsterConfiguration(this IServiceCollection services)
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
                    dest => dest.DocumentsRetrieved,
                    src => GetDocumentsRetrieved(src)
                )
                .Map
                (
                    dest => dest.ProcessingCompleted,
                    src => GetProcessingCompleted(src)
                )
                .Map
                (
                    dest => dest.Documents,
                    src => Enumerable.Empty<DocumentDto>()
                        .Concat(src.CmsDocuments.Adapt<DocumentDto[]>())
                        .Concat(src.PcdRequests.Adapt<DocumentDto[]>())
                        .Concat(
                            (src.DefendantsAndCharges != null && src.DefendantsAndCharges.HasMultipleDefendants
                                    ? new[] { src.DefendantsAndCharges }
                                    : Enumerable.Empty<DefendantsAndChargesEntity>())
                            .Adapt<DocumentDto[]>()
                        )
                //GetDocumentEntities(src)
                );

            TypeAdapterConfig<RecognizePiiEntitiesResultCollection, PiiEntitiesResultCollection>
                .NewConfig()
                .Map
                (
                    dest => dest.Items,
                    src => src.Adapt<List<PiiEntitiesResult>>()
                );

            TypeAdapterConfig<RecognizePiiEntitiesResult, PiiEntitiesResult>
                .NewConfig()
                .Map
                (
                    dest => dest,
                    src => src.Entities.Adapt<PiiResultEntityCollection>()
                );

            TypeAdapterConfig<PiiEntityCollection, PiiResultEntityCollection>
                .NewConfig()
                .Include<PiiEntityCollection, PiiResultEntityCollection>()
                .Map
                (
                    dest => dest,
                    src => src.Adapt<List<PiiResultEntity>>()
                )
                .Map
                (
                    dest => dest.Warnings,
                    src => src.Warnings
                );

            TypeAdapterConfig<PiiEntity, PiiResultEntity>
                .NewConfig()
                .Map
                (
                    dest => dest.Category,
                    src => src.Category.ToString()
                );
        }

        private static DateTime? GetDocumentsRetrieved(CaseDurableEntity caseEntity) =>
            caseEntity.Retrieved.HasValue && caseEntity.Running.HasValue
                ? caseEntity.Running.Value.AddSeconds(caseEntity.Retrieved.Value).ToUniversalTime()
                : null;


        private static DateTime? GetProcessingCompleted(CaseDurableEntity caseEntity) =>
            caseEntity.Retrieved.HasValue && caseEntity.Completed.HasValue
                ? caseEntity.Running.Value.AddSeconds(caseEntity.Completed.Value).ToUniversalTime()
                : null;
    }
}
