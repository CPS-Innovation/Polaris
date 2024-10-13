using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Common.Dto.FeatureFlags;
using Mapster;
using Common.ValueObjects;
using Common.Dto.Tracker;
using Common.Constants;
using System;

namespace coordinator.Durable.Payloads.Domain
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

        [JsonProperty("cmsVersionId")]
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

        [Obsolete("This shouldn't really be a property as it can always be worked out buy convention")]
        [JsonProperty("pdfBlobName")]
        public string PdfBlobName { get; set; }

        [JsonProperty("presentationFlags")]
        public PresentationFlagsDto PresentationFlags { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("conversionStatus")]
        public PdfConversionStatus ConversionStatus { get; set; }

        [JsonProperty("piiCmsVersionId")]
        public long? PiiCmsVersionId { get; set; }
    }
}