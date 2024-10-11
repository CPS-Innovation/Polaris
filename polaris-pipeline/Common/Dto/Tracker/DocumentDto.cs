using System.ComponentModel.DataAnnotations;
using Common.Constants;
using Common.Dto.Document;
using Common.Dto.FeatureFlags;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Common.Dto.Tracker
{
    public class DocumentDto
    {
        public DocumentDto()
        { }

        [JsonProperty("documentId")]
        public string DocumentId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public DocumentStatus Status { get; set; }

        [JsonProperty("versionId")]
        public long VersionId { get; set; }

        [JsonProperty("cmsDocType")]
        public DocumentTypeDto CmsDocType { get; set; }

        [JsonProperty("cmsOriginalFileName")]
        [Required]
        [RegularExpression(@"^.+\.[A-Za-z]{3,4}$")]
        public string CmsOriginalFileName { get; set; }

        [JsonProperty("presentationTitle")]
        public string PresentationTitle { get; set; }

        [JsonProperty("cmsFileCreatedDate")]
        public string CmsFileCreatedDate { get; set; }

        [JsonProperty("pdfBlobName")]
        [JsonIgnore]
        public string PdfBlobName { get; set; }

        [JsonProperty("isOcrProcessed")]
        public bool IsOcrProcessed { get; set; }

        [JsonProperty("categoryListOrder")]
        public int? CategoryListOrder { get; set; }

        [JsonProperty("presentationFlags")]
        public PresentationFlagsDto PresentationFlags { get; set; }

        [JsonProperty("parentDocumentId")]
        public string PolarisParentDocumentId { get; set; }

        [JsonProperty("cmsParentDocumentId")]
        [JsonIgnore]
        public string CmsParentDocumentId { get; set; }

        [JsonProperty("witnessId")]
        public int? WitnessId { get; set; }

        [JsonProperty("hasFailedAttachments")]
        public bool HasFailedAttachments { get; set; }

        [JsonProperty("hasNotes")]
        public bool HasNotes { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("conversionStatus")]
        public PdfConversionStatus ConversionStatus { get; set; }

        [JsonProperty("piiVersionId")]
        public int? PiiVersionId { get; set; }

        [JsonProperty("isUnused")]
        public bool IsUnused { get; set; }

        [JsonProperty("isInbox")]
        public bool IsInbox { get; set; }

        [JsonProperty("classification")]
        public string Classification { get; set; }

        [JsonProperty("isWitnessManagement")]
        public bool IsWitnessManagement { get; set; }

        [JsonProperty("canReclassify")]
        public bool CanReclassify { get; set; }

        [JsonProperty("canRename")]
        public bool CanRename { get; set; }

        [JsonProperty("renameStatus")]
        public string RenameStatus { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }
    }
}