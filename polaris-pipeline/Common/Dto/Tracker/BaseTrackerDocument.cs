using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using Common.Dto.Document;
using Common.Dto.FeatureFlags;

namespace Common.Dto.Tracker
{
    public class BaseTrackerDocumentDto
    {
        public BaseTrackerDocumentDto(
            Guid polarisDocumentId,
            int polarisDocumentVersionId,
            string cmsDocumentId,
            long cmsVersionId,
            PresentationFlagsDto presentationFlags)
        {
            PolarisDocumentId = polarisDocumentId;
            PolarisDocumentVersionId = polarisDocumentVersionId;
            CmsDocumentId = cmsDocumentId;
            CmsVersionId = cmsVersionId;
            PresentationFlags = presentationFlags;
            Status = TrackerDocumentStatus.New;
        }

        [JsonProperty("polarisDocumentId")]
        public Guid PolarisDocumentId { get; set; }

        [JsonProperty("polarisDocumentVersionId")]
        public int PolarisDocumentVersionId { get; set; }

        // Todo - don't send to client
        [JsonProperty("cmsDocumentId")]
        public string CmsDocumentId { get; set; }

        // Todo - don't send to client
        [JsonProperty("cmsVersionId")]
        public long CmsVersionId { get; set; }

        [JsonProperty("pdfBlobName")]
        public string PdfBlobName { get; set; }

        [JsonProperty("isPdfAvailable")]
        public bool IsPdfAvailable { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public TrackerDocumentStatus Status { get; set; }

        [JsonProperty("presentationFlags")]
        public PresentationFlagsDto PresentationFlags { get; set; }
    }
}