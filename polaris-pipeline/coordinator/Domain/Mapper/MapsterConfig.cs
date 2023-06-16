using Common.Dto.Document;
using Common.Dto.Tracker;
using coordinator.Functions.DurableEntity.Entity;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace coordinator.Domain.Mapper
{
    public static class MapsterConfig
    {
        // dd/mm/yyyy
        private static Regex dateRegex = new Regex(@"^([0-2]\d|3[01])\/((0[1-9])|(1[0-2]))\/\d{4}$", RegexOptions.Compiled);

        public static void RegisterMapsterConfiguration(this IServiceCollection services)
        {
            TypeAdapterConfig<TrackerEntity, TrackerDto>
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
                CmsFileCreatedDate = StandardiseDayMonthYearDateFormat(pcdRequest.PcdRequest.DecisionRequested),
                CmsOriginalFileName = Path.GetFileName(pcdRequest.PdfBlobName) ?? "(Pending) PCD.pdf",
                PresentationTitle = Path.GetFileNameWithoutExtension(pcdRequest.PdfBlobName) ?? "(Pending) PCD",
                PresentationFlags = pcdRequest.PresentationFlags,
                PdfBlobName = pcdRequest.PdfBlobName,
                IsPdfAvailable = pcdRequest.IsPdfAvailable,
                Status = pcdRequest.Status
            };
        }

        // dd/mm/yyyy -> yyyy-mm-dd
        private static string StandardiseDayMonthYearDateFormat(string decisionRequested)
        {
            if(!IsDdYyMmmmDate(decisionRequested))
                return decisionRequested;

            var dateParts = decisionRequested.Split('/');
            var year = dateParts[2];
            var month = $"{int.Parse(dateParts[1]):D2}";
            var day = $"{int.Parse(dateParts[0]):D2}";
            return $"{year}-{month}-{day}";
        }

        private static bool IsDdYyMmmmDate(string input)
        {
            return dateRegex.IsMatch(input);
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
