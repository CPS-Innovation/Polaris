using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Common.Dto.FeatureFlags;
using Mapster;
using Common.ValueObjects;

namespace Common.Dto.Tracker
{
    public class BaseTrackerDocumentDto
    {
        public BaseTrackerDocumentDto()
        { }

        public BaseTrackerDocumentDto(
            PolarisDocumentId polarisDocumentId,
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

        [JsonIgnore]
        public PolarisDocumentId PolarisDocumentId { get; set; }

        [JsonProperty("polarisDocumentId")]
        public string PolarisDocumentIdValue
        {
            get
            { 
                return PolarisDocumentId.ToString(); 
            }
            set
            {
                PolarisDocumentId = new PolarisDocumentId(value);
            }
        }

        [JsonProperty("polarisDocumentVersionId")]
        public int PolarisDocumentVersionId { get; set; }

        [JsonProperty("cmsDocumentId")]
        [AdaptIgnore]
        public string CmsDocumentId { get; set; }

        // Todo - don't send to client
        [JsonProperty("cmsVersionId")]
        [AdaptIgnore]
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