using Newtonsoft.Json;

namespace Common.Dto.Response;

public class DdeiCaseDocumentResponse
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
}
