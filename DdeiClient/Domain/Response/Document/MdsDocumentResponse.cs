using System.Text.Json.Serialization;

namespace Ddei.Domain.Response.Document;

public class MdsDocumentResponse
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("versionId")]
    public long VersionId { get; set; }

    [JsonPropertyName("presentationTitle")]
    public string PresentationTitle { get; set; }

    [JsonPropertyName("path")]
    public string Path { get; set; }

    [JsonPropertyName("originalFileName")]
    public string OriginalFileName { get; set; }

    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; }

    [JsonPropertyName("fileExtension")]
    public string FileExtension { get; set; }

    [JsonPropertyName("cmsDocCategory")]
    public string CmsDocCategory { get; set; }

    [JsonPropertyName("typeId")]
    public int? DocumentTypeId { get; set; }

    [JsonPropertyName("type")]
    public string DocumentType { get; set; }

    [JsonPropertyName("date")]
    public string DocumentDate { get; set; }

    [JsonPropertyName("isOcrProcessed")]
    public bool? IsOcrProcessed { get; set; }

    [JsonPropertyName("isDispatched")]
    public bool IsDispatched { get; set; }

    [JsonPropertyName("categoryListOrder")]
    public int? CategoryListOrder { get; set; }

    [JsonPropertyName("parentId")]
    public long? ParentId { get; set; }

    [JsonPropertyName("witnessId")]
    public int? WitnessId { get; set; }

    [JsonPropertyName("hasFailedAttachments")]
    public bool HasFailedAttachments { get; set; }

    [JsonPropertyName("hasNotes")]
    public bool HasNotes { get; set; }

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

    [JsonPropertyName("title")]
    public string Title { get; set; }
}
