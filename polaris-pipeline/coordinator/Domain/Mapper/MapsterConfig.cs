using Common.Domain.Entity;
using Common.Dto.Document;
using Common.Dto.Tracker;
using coordinator.Functions.DurableEntity.Entity;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;

namespace coordinator.Domain.Mapper
{
    public static class MapsterConfig
    {
        public static void RegisterMapsterConfiguration(this IServiceCollection services)
        {
            TypeAdapterConfig<(CaseDurableEntity CaseEntity, CaseRefreshLogsDurableEntity CaseRefreshLogsEntity), TrackerDto>
                .NewConfig()
                .Map
                (
                    dest => dest,
                    src => src.CaseEntity
                )
                .Map
                (
                    dest => dest.VersionId,
                    src => src.CaseEntity.Version
                )
                .Map
                (
                    dest => dest.DocumentsRetrieved,
                    src => GetDocumentsRetrieved(src.CaseEntity)
                )
                .Map
                (
                    dest => dest.ProcessingCompleted,
                    src => GetProcessingCompleted(src.CaseEntity)
                )
                .Map
                (
                    dest => dest.Documents,
                    src =>
                        src.CaseEntity.PcdRequests
                            .Select(pcdRequest => ConvertToTrackerCmsDocumentDto(pcdRequest))
                            .Concat(ConvertToTrackerCmsDocumentDto(src.CaseEntity.DefendantsAndCharges))
                            .Concat(src.CaseEntity.CmsDocuments)
                )
                .Map
                (
                    dest => dest.Logs.Case,
                    src => src.CaseRefreshLogsEntity.Case
                )
                .Map
                (
                    dest => dest.Logs.Documents,
                    src => src.CaseRefreshLogsEntity.Documents
                );
        }

        private static DateTime? GetDocumentsRetrieved(CaseDurableEntity caseEntity)
        {
            if(caseEntity.Running != null && caseEntity.Retrieved.HasValue)
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
                PolarisDocumentVersionId = pcdRequest.PolarisDocumentVersionId,
                CmsDocumentId = pcdRequest.CmsDocumentId,
                CmsVersionId = pcdRequest.CmsVersionId,
                CmsDocType = new DocumentTypeDto("PCD", null, "Review"),
                CmsFileCreatedDate = pcdRequest.PcdRequest.DecisionRequested,
                CmsOriginalFileName = Path.GetFileName(pcdRequest.PdfBlobName) ?? "(Pending) PCD.pdf",
                PresentationTitle = Path.GetFileNameWithoutExtension(pcdRequest.PdfBlobName) ?? "(Pending) PCD",
                PresentationFlags = pcdRequest.PresentationFlags,
                PdfBlobName = pcdRequest.PdfBlobName,
                IsPdfAvailable = pcdRequest.IsPdfAvailable,
                Status = pcdRequest.Status
            };
        }

        private static CmsDocumentEntity[] ConvertToTrackerCmsDocumentDto(DefendantsAndChargesEntity defendantsAndCharges)
        {
            if(defendantsAndCharges == null) 
                return new CmsDocumentEntity[0];

            return new CmsDocumentEntity[1] 
            {
                new CmsDocumentEntity
                {
                    PolarisDocumentId = defendantsAndCharges.PolarisDocumentId,
                    PolarisDocumentVersionId = defendantsAndCharges.PolarisDocumentVersionId,
                    CmsDocumentId = defendantsAndCharges.CmsDocumentId,
                    CmsVersionId = defendantsAndCharges.CmsVersionId,
                    CmsDocType = new DocumentTypeDto("DAC", null, "Review"),
                    CmsFileCreatedDate = DateTime.Today.ToString("yyyy-MM-dd"),
                    CmsOriginalFileName = Path.GetFileName(defendantsAndCharges.PdfBlobName) ?? "(Pending) DAC.pdf",
                    PresentationTitle = Path.GetFileNameWithoutExtension(defendantsAndCharges.PdfBlobName) ?? "(Pending) DAC",
                    PresentationFlags = defendantsAndCharges.PresentationFlags,
                    PdfBlobName = defendantsAndCharges.PdfBlobName,
                    IsPdfAvailable = defendantsAndCharges.IsPdfAvailable,
                    Status = defendantsAndCharges.Status 
                }
            };
        }

    }
}
