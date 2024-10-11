using Newtonsoft.Json;

namespace Ddei.Domain.Response.Document;

public class DdeiDocumentResponse
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("versionId")]
    public long VersionId { get; set; }

    [JsonProperty("presentationTitle")]
    public string PresentationTitle { get; set; }

    [JsonProperty("path")]
    public string Path { get; set; }

    [JsonProperty("originalFileName")]
    public string OriginalFileName { get; set; }

    [JsonProperty("mimeType")]
    public string MimeType { get; set; }

    [JsonProperty("fileExtension")]
    public string FileExtension { get; set; }

    [JsonProperty("cmsDocCategory")]
    public string CmsDocCategory { get; set; }

    [JsonProperty("typeId")]
    public string DocumentTypeId { get; set; }

    [JsonProperty("type")]
    public string DocumentType { get; set; }

    [JsonProperty("date")]
    public string DocumentDate { get; set; }

    [JsonProperty("isOcrProcessed")]
    public bool? IsOcrProcessed { get; set; }

    [JsonProperty("isDispatched")]
    public bool IsDispatched { get; set; }

    [JsonProperty("categoryListOrder")]
    public int? CategoryListOrder { get; set; }

    [JsonProperty("parentId")]
    public long? ParentId { get; set; }

    [JsonProperty("witnessId")]
    public int? WitnessId { get; set; }

    [JsonProperty("hasFailedAttachments")]
    public bool HasFailedAttachments { get; set; }

    [JsonProperty("hasNotes")]
    public bool HasNotes { get; set; }

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
