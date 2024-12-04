using Common.Dto.Response.Documents;
using Common.Constants;
using Common.Dto.Response.Document.FeatureFlags;
using System.Text.Json.Serialization;

namespace coordinator.Durable.Payloads.Domain
{
    public abstract class BaseDocumentEntity
    {
        public BaseDocumentEntity() 
        {
        }

        protected BaseDocumentEntity(
            long cmsDocumentId,
            long versionId,
            PresentationFlagsDto presentationFlags)
        {
            CmsDocumentId = cmsDocumentId;
            VersionId = versionId;
            PresentationFlags = presentationFlags;
            Status = DocumentStatus.New;
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("status")]
        public DocumentStatus Status { get; set; }

        [JsonPropertyName("documentId")]
        public abstract string DocumentId { get; }

        [JsonPropertyName("cmsDocumentId")]
        //[AdaptIgnore]
        public long CmsDocumentId { get; set; }

        [JsonPropertyName("versionId")]
        public long VersionId { get; set; }

        [JsonPropertyName("presentationFlags")]
        public PresentationFlagsDto PresentationFlags { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("conversionStatus")]
        public PdfConversionStatus ConversionStatus { get; set; }

        [JsonPropertyName("piiVersionId")]
        public long? PiiVersionId { get; set; }
    }
}