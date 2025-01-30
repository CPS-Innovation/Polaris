using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Common.Dto.Response.Documents;
using Common.Constants;

namespace coordinator.Durable.Payloads.Domain
{
    public abstract class BaseDocumentEntity
    {
        public BaseDocumentEntity()
        { }

        protected BaseDocumentEntity(
            long cmsDocumentId,
            long versionId)
        {
            CmsDocumentId = cmsDocumentId;
            VersionId = versionId;
            Status = DocumentStatus.New;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public DocumentStatus Status { get; set; }

        [JsonProperty("documentId")]
        public abstract string DocumentId { get; }

        [JsonProperty("cmsDocumentId")]
        //[AdaptIgnore]
        public long CmsDocumentId { get; set; }

        [JsonProperty("versionId")]
        public long VersionId { get; set; }
        public PdfConversionStatus ConversionStatus { get; set; }
    }
}