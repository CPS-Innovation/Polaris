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

        // private static List<CmsDocumentEntity> GetDocumentEntities(CaseDurableEntity caseEntity)
        // {
        //     var documents = new List<CmsDocumentEntity>();

        //     if (caseEntity.CmsDocuments?.Any() == true)
        //         documents.AddRange(caseEntity.CmsDocuments);

        //     if (caseEntity.DefendantsAndCharges != null && caseEntity.DefendantsAndCharges.HasMultipleDefendants)
        //     {
        //         var defendantsAndChargesDocument = ConvertToTrackerCmsDocumentDto(caseEntity.DefendantsAndCharges);
        //         documents.AddRange(defendantsAndChargesDocument);
        //     }

        //     if (caseEntity.PcdRequests?.Any() == true)
        //     {
        //         var pcdRequestDocuments = caseEntity.PcdRequests.Select(pcdRequest => ConvertToTrackerCmsDocumentDto(pcdRequest));
        //         documents.AddRange(pcdRequestDocuments);
        //     }

        //     return documents;
        // }

        private static DateTime? GetDocumentsRetrieved(CaseDurableEntity caseEntity) =>
            caseEntity.Retrieved.HasValue && caseEntity.Running.HasValue
                ? caseEntity.Running.Value.AddSeconds(caseEntity.Retrieved.Value).ToUniversalTime()
                : null;


        private static DateTime? GetProcessingCompleted(CaseDurableEntity caseEntity) =>
            caseEntity.Retrieved.HasValue && caseEntity.Completed.HasValue
                ? caseEntity.Running.Value.AddSeconds(caseEntity.Completed.Value).ToUniversalTime()
                : null;

        // private static CmsDocumentEntity ConvertToTrackerCmsDocumentDto(PcdRequestEntity pcdRequest)
        // {
        //     return new CmsDocumentEntity(pcdRequest.CmsDocumentId, pcdRequest.VersionId, pcdRequest.PresentationFlags)
        //     {
        //         //CmsDocType = new DocumentTypeDto("PCD", null, "Review"),
        //         //CmsFileCreatedDate = pcdRequest.PcdRequest.DecisionRequested,
        //         //CmsOriginalFileName = Path.GetFileName(pcdRequest.PdfBlobName) ?? $"(Pending) PCD.pdf",
        //         // PresentationTitle = Path.GetFileNameWithoutExtension(pcdRequest.PdfBlobName)
        //         //     // Temporary hack: we need to rationalise the way these are named.  In the meantime, to prevent
        //         //     //  false-positive name update notifications being shown in the UI, we make sure the interim name
        //         //     //  on th PCS request is the same as the eventual name derived from the blob name.
        //         //     ?? $"CMS-{pcdRequest.DocumentId}",
        //         //PdfBlobName = pcdRequest.PdfBlobName,
        //         //Status = pcdRequest.Status
        //     };
        // }

        // private static CmsDocumentEntity[] ConvertToTrackerCmsDocumentDto(DefendantsAndChargesEntity defendantsAndCharges)
        // {
        //     if (defendantsAndCharges == null)
        //         return new CmsDocumentEntity[0];

        //     return new CmsDocumentEntity[1]
        //     {
        //         new CmsDocumentEntity(defendantsAndCharges.CmsDocumentId, defendantsAndCharges.VersionId,defendantsAndCharges.PresentationFlags)
        //         {
        //             // CmsDocType = new DocumentTypeDto("DAC", null, "Review"),
        //             //CmsFileCreatedDate = DateTime.Today.ToString("yyyy-MM-dd"),
        //             //CmsOriginalFileName = Path.GetFileName(defendantsAndCharges.PdfBlobName) ?? "(Pending) DAC.pdf",
        //             // PresentationTitle = Path.GetFileNameWithoutExtension(defendantsAndCharges.PdfBlobName) 
        //             //     // Temporary hack: we need to rationalise the way these are named.  In the meantime, to prevent
        //             //     //  false-positive name update notifications being shown in the UI, we make sure the interim name
        //             //     //  on th PCS request is the same as the eventual name derived from the blob name.
        //             //     ?? "CMS-DAC",
        //             //PdfBlobName = defendantsAndCharges.PdfBlobName,
        //             //Status = defendantsAndCharges.Status
        //         }
        //     };
        // }

    }
}
