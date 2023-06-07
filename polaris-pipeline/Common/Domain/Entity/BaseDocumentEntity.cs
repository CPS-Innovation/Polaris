using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Common.Dto.FeatureFlags;
using Mapster;
using Common.ValueObjects;
using Common.Dto.Tracker;

namespace Common.Domain.Entity
{
    public class BaseDocumentEntity
    {
        public BaseDocumentEntity()
        { }

        public BaseDocumentEntity(
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
            Status = DocumentStatus.New;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public DocumentStatus Status { get; set; }

        [JsonProperty("cmsDocumentId")]
        [AdaptIgnore]
        public string CmsDocumentId { get; set; }

        // Todo - don't send to client
        [JsonProperty("cmsVersionId")]
        [AdaptIgnore]
        public long CmsVersionId { get; set; }

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

        [JsonProperty("isPdfAvailable")]
        public bool IsPdfAvailable { get; set; }

        [JsonProperty("pdfBlobName")]
        public string PdfBlobName { get; set; }

        [JsonProperty("presentationFlags")]
        public PresentationFlagsDto PresentationFlags { get; set; }
    }
}