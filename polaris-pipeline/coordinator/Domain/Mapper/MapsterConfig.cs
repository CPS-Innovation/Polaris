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
            TypeAdapterConfig<TrackerEntity, TrackerDto>
                .NewConfig()
                .Map(
                        dest => dest.Documents,
                        src => src.PcdRequests.Select
                            (
                                pcdRequest =>
                                new TrackerCmsDocumentDto
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
                                }
                            ).Concat(src.Documents)
                    );
        }
    }
}
