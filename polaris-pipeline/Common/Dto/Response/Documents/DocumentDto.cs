using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Common.Constants;
using Common.Dto.Response.Document;
using Common.Dto.Response.Document.FeatureFlags;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Common.Dto.Response.Documents
{
    public class DocumentDto
    {
        public DocumentDto()
        {
        }

        [JsonProperty("documentId")]
        [JsonPropertyName("documentId")]
        public string DocumentId { get; set; }

        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonProperty("status")]
        [JsonPropertyName("status")]
        public DocumentStatus Status { get; set; }

        [JsonProperty("versionId")]
        [JsonPropertyName("versionId")]
        public long VersionId { get; set; }

        [JsonProperty("cmsDocType")]
        [JsonPropertyName("cmsDocType")]
        public DocumentTypeDto CmsDocType { get; set; }

        [JsonProperty("cmsOriginalFileName")]
        [JsonPropertyName("cmsOriginalFileName")]
        [Required]
        [RegularExpression(@"^.+\.[A-Za-z]{3,4}$")]
        public string CmsOriginalFileName { get; set; }

        [JsonProperty("presentationTitle")]
        [JsonPropertyName("presentationTitle")]
        public string PresentationTitle { get; set; }

        [JsonProperty("cmsFileCreatedDate")]
        [JsonPropertyName("cmsFileCreatedDate")]
        public string CmsFileCreatedDate { get; set; }

        [JsonProperty("pdfBlobName")]
        [JsonPropertyName("pdfBlobName")]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string PdfBlobName { get; set; }

        [JsonProperty("isOcrProcessed")]
        [JsonPropertyName("isOcrProcessed")]
        public bool IsOcrProcessed { get; set; }

        [JsonProperty("categoryListOrder")]
        [JsonPropertyName("categoryListOrder")]
        public int? CategoryListOrder { get; set; }

        [JsonProperty("presentationFlags")]
        [JsonPropertyName("presentationFlags")]
        public PresentationFlagsDto PresentationFlags { get; set; }

        [JsonProperty("parentDocumentId")]
        [JsonPropertyName("parentDocumentId")]
        public string ParentDocumentId { get; set; }

        [JsonProperty("witnessId")]
        [JsonPropertyName("witnessId")]
        public int? WitnessId { get; set; }

        [JsonProperty("hasFailedAttachments")]
        [JsonPropertyName("hasFailedAttachments")]
        public bool HasFailedAttachments { get; set; }

        [JsonProperty("hasNotes")]
        [JsonPropertyName("hasNotes")]
        public bool HasNotes { get; set; }

        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonProperty("conversionStatus")]
        public PdfConversionStatus ConversionStatus { get; set; }

        [JsonProperty("piiVersionId")]
        [JsonPropertyName("piiVersionId")]
        public int? PiiVersionId { get; set; }

        [JsonProperty("isUnused")]
        [JsonPropertyName("isUnused")]
        public bool IsUnused { get; set; }

        [JsonProperty("isInbox")]
        [JsonPropertyName("isInbox")]
        public bool IsInbox { get; set; }

        [JsonProperty("classification")]
        [JsonPropertyName("classification")]
        public string Classification { get; set; }

        [JsonProperty("isWitnessManagement")]
        [JsonPropertyName("isWitnessManagement")]
        public bool IsWitnessManagement { get; set; }

        [JsonProperty("canReclassify")]
        [JsonPropertyName("canReclassify")]
        public bool CanReclassify { get; set; }

        [JsonProperty("canRename")]
        [JsonPropertyName("canRename")]
        public bool CanRename { get; set; }

        [JsonProperty("renameStatus")]
        [JsonPropertyName("renameStatus")]
        public string RenameStatus { get; set; }

        [JsonProperty("reference")]
        [JsonPropertyName("reference")]
        public string Reference { get; set; }
    }
}