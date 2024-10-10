using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Common.Dto.FeatureFlags;
using Mapster;
using Common.Dto.Tracker;
using Common.Constants;
using System;
using System.Text.RegularExpressions;

namespace coordinator.Durable.Payloads.Domain
{
    public class BaseDocumentEntity
    {
        private const string DocumentIdPatternRegex = $@"^({PolarisDocumentTypePrefixes.CmsDocument}|{PolarisDocumentTypePrefixes.PcdRequest}|{PolarisDocumentTypePrefixes.DefendantsAndCharges})-([0-9]+)$";
        public BaseDocumentEntity()
        { }

        protected BaseDocumentEntity(
            string documentId,
            long versionId,
            PresentationFlagsDto presentationFlags)
        {
            AssertDocumentIdFormat(documentId);
            DocumentId = documentId;
            VersionId = versionId;
            PresentationFlags = presentationFlags;
            Status = DocumentStatus.New;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public DocumentStatus Status { get; set; }

        [JsonProperty("cmsDocumentId")]
        [AdaptIgnore]
        public string CmsDocumentId
        {
            get
            {
                return DocumentId?.Split('-')[1];
            }
        }

        [JsonProperty("versionId")]
        public long VersionId { get; set; }

        private string documentId;
        [JsonProperty("documentId")]
        public string DocumentId
        {
            get
            {
                return documentId;
            }
            internal set
            {
                AssertDocumentIdFormat(value);
                documentId = value;
            }
        }

        [Obsolete("This shouldn't really be a property as it can always be worked out buy convention")]
        [JsonProperty("pdfBlobName")]
        public string PdfBlobName { get; set; }

        [JsonProperty("presentationFlags")]
        public PresentationFlagsDto PresentationFlags { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("conversionStatus")]
        public PdfConversionStatus ConversionStatus { get; set; }

        [JsonProperty("piiVersionId")]
        public long? PiiVersionId { get; set; }

        private static void AssertDocumentIdFormat(string documentId)
        {
            if (string.IsNullOrEmpty(documentId))
            {
                throw new ArgumentNullException("Parameter cannot be null or empty", nameof(documentId));
            }
            if (!Regex.Match(documentId, DocumentIdPatternRegex).Success)
            {
                throw new ArgumentException("Parameter must be in the format of {CMS|PCD|DAC}-[0-9]+", nameof(documentId));
            }
        }
    }
}