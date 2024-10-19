using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Azure.AI.TextAnalytics;
using Common.Services.PiiService.Domain;
using Mapster;


namespace Common.Services.PiiService.Mappers
{
    public static class MapsterConfig
    {
        public static void RegisterPiiMapsterConfiguration(this IServiceCollection services)
        {

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
    }
}
