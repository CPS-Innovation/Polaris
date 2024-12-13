using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Common.Constants;
using Common.Dto.Response.Document;
using Common.Dto.Response.Document.FeatureFlags;

namespace Common.Dto.Response.Documents
{
    public class DocumentDto
    {
        public DocumentDto()
        {
        }

        [JsonPropertyName("documentId")]
        public string DocumentId { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("status")]
        public DocumentStatus Status { get; set; }

        [JsonPropertyName("versionId")]
        public long VersionId { get; set; }

        [JsonPropertyName("cmsDocType")]
        public DocumentTypeDto CmsDocType { get; set; }

        [JsonPropertyName("cmsOriginalFileName")]
        [Required]
        [RegularExpression(@"^.+\.[A-Za-z]{3,4}$")]
        public string CmsOriginalFileName { get; set; }

        [JsonPropertyName("presentationTitle")]
        public string PresentationTitle { get; set; }

        [JsonPropertyName("cmsFileCreatedDate")]
        public string CmsFileCreatedDate { get; set; }

        [JsonPropertyName("pdfBlobName")]
        [JsonIgnore]
        public string PdfBlobName { get; set; }

        [JsonPropertyName("isOcrProcessed")]
        public bool IsOcrProcessed { get; set; }

        [JsonPropertyName("categoryListOrder")]
        public int? CategoryListOrder { get; set; }

        [JsonPropertyName("presentationFlags")]
        public PresentationFlagsDto PresentationFlags { get; set; }

        [JsonPropertyName("parentDocumentId")]
        public string ParentDocumentId { get; set; }

        [JsonPropertyName("witnessId")]
        public int? WitnessId { get; set; }

        [JsonPropertyName("hasFailedAttachments")]
        public bool HasFailedAttachments { get; set; }

        [JsonPropertyName("hasNotes")]
        public bool HasNotes { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("conversionStatus")]
        public PdfConversionStatus ConversionStatus { get; set; }

        [JsonPropertyName("piiVersionId")]
        public int? PiiVersionId { get; set; }

        [JsonPropertyName("isUnused")]
        public bool IsUnused { get; set; }

        [JsonPropertyName("isInbox")]
        public bool IsInbox { get; set; }

        [JsonPropertyName("classification")]
        public string Classification { get; set; }

        [JsonPropertyName("isWitnessManagement")]
        public bool IsWitnessManagement { get; set; }

        [JsonPropertyName("canReclassify")]
        public bool CanReclassify { get; set; }

        [JsonPropertyName("canRename")]
        public bool CanRename { get; set; }

        [JsonPropertyName("renameStatus")]
        public string RenameStatus { get; set; }

        [JsonPropertyName("reference")]
        public string Reference { get; set; }
    }
}