using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Azure.AI.TextAnalytics;
using coordinator.Domain;
using coordinator.Durable.Entity;
using coordinator.Durable.Payloads.Domain;
using Common.Dto.Document;
using Common.Dto.Tracker;
using Mapster;

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
                    dest => dest.VersionId,
                    src => src.Version
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
                    src => GetDocumentEntities(src)
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

        private static List<CmsDocumentEntity> GetDocumentEntities(CaseDurableEntity caseEntity)
        {
            var documents = new List<CmsDocumentEntity>();

            if (caseEntity.CmsDocuments?.Any() == true)
                documents.AddRange(caseEntity.CmsDocuments);

            if (caseEntity.DefendantsAndCharges != null && caseEntity.DefendantsAndCharges.HasMultipleDefendants)
            {
                var defendantsAndChargesDocument = ConvertToTrackerCmsDocumentDto(caseEntity.DefendantsAndCharges);
                documents.AddRange(defendantsAndChargesDocument);
            }

            if (caseEntity.PcdRequests?.Any() == true)
            {
                var pcdRequestDocuments = caseEntity.PcdRequests.Select(pcdRequest => ConvertToTrackerCmsDocumentDto(pcdRequest));
                documents.AddRange(pcdRequestDocuments);
            }

            return documents;
        }

        private static DateTime? GetDocumentsRetrieved(CaseDurableEntity caseEntity)
        {
            if (caseEntity.Running != null && caseEntity.Retrieved.HasValue)
                return caseEntity.Running?.AddSeconds(caseEntity.Retrieved.Value).ToUniversalTime();

            return null;
        }

        private static DateTime? GetProcessingCompleted(CaseDurableEntity caseEntity)
        {
            if (caseEntity.Running != null && caseEntity.Completed.HasValue)
                return caseEntity.Running?.AddSeconds(caseEntity.Completed.Value).ToUniversalTime();

            return null;
        }

        private static CmsDocumentEntity ConvertToTrackerCmsDocumentDto(PcdRequestEntity pcdRequest)
        {
            return new CmsDocumentEntity
            {
                PolarisDocumentId = pcdRequest.PolarisDocumentId,
                CmsDocumentId = pcdRequest.CmsDocumentId,
                CmsVersionId = pcdRequest.CmsVersionId,
                CmsDocType = new DocumentTypeDto("PCD", null, "Review"),
                CmsFileCreatedDate = pcdRequest.PcdRequest.DecisionRequested,
                CmsOriginalFileName = Path.GetFileName(pcdRequest.PdfBlobName) ?? $"(Pending) PCD.pdf",
                PresentationTitle = Path.GetFileNameWithoutExtension(pcdRequest.PdfBlobName)
                    // Temporary hack: we need to rationalise the way these are named.  In the meantime, to prevent
                    //  false-positive name update notifications being shown in the UI, we make sure the interim name
                    //  on th PCS request is the same as the eventual name derived from the blob name.
                    ?? $"CMS-{pcdRequest.PolarisDocumentId}",
                PresentationFlags = pcdRequest.PresentationFlags,
                PdfBlobName = pcdRequest.PdfBlobName,
                Status = pcdRequest.Status
            };
        }

        private static CmsDocumentEntity[] ConvertToTrackerCmsDocumentDto(DefendantsAndChargesEntity defendantsAndCharges)
        {
            if (defendantsAndCharges == null)
                return new CmsDocumentEntity[0];

            return new CmsDocumentEntity[1]
            {
                new CmsDocumentEntity
                {
                    PolarisDocumentId = defendantsAndCharges.PolarisDocumentId,
                    CmsDocumentId = defendantsAndCharges.CmsDocumentId,
                    CmsVersionId = defendantsAndCharges.CmsVersionId,
                    CmsDocType = new DocumentTypeDto("DAC", null, "Review"),
                    CmsFileCreatedDate = DateTime.Today.ToString("yyyy-MM-dd"),
                    CmsOriginalFileName = Path.GetFileName(defendantsAndCharges.PdfBlobName) ?? "(Pending) DAC.pdf",
                    PresentationTitle = Path.GetFileNameWithoutExtension(defendantsAndCharges.PdfBlobName) 
                        // Temporary hack: we need to rationalise the way these are named.  In the meantime, to prevent
                        //  false-positive name update notifications being shown in the UI, we make sure the interim name
                        //  on th PCS request is the same as the eventual name derived from the blob name.
                        ?? "CMS-DAC",
                    PresentationFlags = defendantsAndCharges.PresentationFlags,
                    PdfBlobName = defendantsAndCharges.PdfBlobName,
                    Status = defendantsAndCharges.Status
                }
            };
        }

    }
}
