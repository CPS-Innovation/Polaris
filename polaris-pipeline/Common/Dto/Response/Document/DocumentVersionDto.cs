using System.ComponentModel.DataAnnotations;
using Common.Validators;
using Newtonsoft.Json;

namespace Common.Dto.Response.Document;

public class DocumentVersionDto
{
    public DocumentVersionDto(string documentId, long versionId)
    {
        DocumentId = documentId;
        VersionId = versionId;
    }

    [JsonProperty("documentId")]
    [Required]
    public string DocumentId { get; set; }

    [JsonProperty("versionId")]
    [RequiredLongGreaterThanZero]
    public long VersionId { get; set; }
}