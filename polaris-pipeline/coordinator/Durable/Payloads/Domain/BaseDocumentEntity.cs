using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Common.Dto.Tracker;
using Common.Constants;
using System;
using Common.Dto.Response.Document.FeatureFlags;

namespace coordinator.Durable.Payloads.Domain
{
    public abstract class BaseDocumentEntity
    {
        public BaseDocumentEntity()
        { }

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

        [Obsolete("This shouldn't really be a property as it can always be worked out buy convention")]
        [JsonProperty("pdfBlobName")]
        public string PdfBlobName { get; set; }

        [JsonProperty("presentationFlags")]
        public PresentationFlagsDto PresentationFlags { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("conversionStatus")]
        public PdfConversionStatus ConversionStatus { get; set; }

        [JsonProperty("presentationTitle")]
        public string PresentationTitle { get; set; }

        [JsonProperty("piiVersionId")]
        public long? PiiVersionId { get; set; }
    }
}