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
            TypeAdapterConfig<CaseEntity, TrackerDto>
                .NewConfig()
                .Map(
                        dest => dest.Documents,
                        src => 
                            src.PcdRequests
                                .Select(pcdRequest => ConvertToTrackerCmsDocumentDto(pcdRequest))
                                .Concat(ConvertToTrackerCmsDocumentDto(src.DefendantsAndCharges))
                                .Concat(src.CmsDocuments)
                    );
        }

        private static TrackerCmsDocumentDto ConvertToTrackerCmsDocumentDto(TrackerPcdRequestDto pcdRequest)
        {
            return new TrackerCmsDocumentDto
            {
                PolarisDocumentId = pcdRequest.PolarisDocumentId,
                PolarisDocumentVersionId = pcdRequest.PolarisDocumentVersionId,
                CmsDocumentId = pcdRequest.CmsDocumentId,
                CmsVersionId = pcdRequest.CmsVersionId,
                CmsDocType = new DocumentTypeDto("PCD", null, "Review"),
                CmsFileCreatedDate = DateTime.Today.ToString("yyyy-MM-dd"),
                CmsOriginalFileName = Path.GetFileName(pcdRequest.PdfBlobName) ?? "(Pending) PCD.pdf",
                PresentationFlags = pcdRequest.PresentationFlags,
                PdfBlobName = pcdRequest.PdfBlobName,
                IsPdfAvailable = pcdRequest.IsPdfAvailable,
                Status = pcdRequest.Status
            };
        }

        private static TrackerCmsDocumentDto[] ConvertToTrackerCmsDocumentDto(TrackerDefendantsAndChargesDto defendantsAndCharges)
        {
            if(defendantsAndCharges == null) 
                return new TrackerCmsDocumentDto[0];

            return new TrackerCmsDocumentDto[1] 
            {
                new TrackerCmsDocumentDto
                {
                    PolarisDocumentId = defendantsAndCharges.PolarisDocumentId,
                    PolarisDocumentVersionId = defendantsAndCharges.PolarisDocumentVersionId,
                    CmsDocumentId = defendantsAndCharges.CmsDocumentId,
                    CmsVersionId = defendantsAndCharges.CmsVersionId,
                    CmsDocType = new DocumentTypeDto("DAC", null, "Review"),
                    CmsFileCreatedDate = DateTime.Today.ToString("yyyy-MM-dd"),
                    CmsOriginalFileName = Path.GetFileName(defendantsAndCharges.PdfBlobName) ?? "(Pending) DAC.pdf",
                    PresentationFlags = defendantsAndCharges.PresentationFlags,
                    PdfBlobName = defendantsAndCharges.PdfBlobName,
                    IsPdfAvailable = defendantsAndCharges.IsPdfAvailable,
                    Status = defendantsAndCharges.Status 
                }
            };
        }

    }
}
