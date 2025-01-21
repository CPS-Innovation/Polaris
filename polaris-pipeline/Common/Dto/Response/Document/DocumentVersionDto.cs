using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Common.Validators;

namespace Common.Dto.Response.Document;

public class DocumentVersionDto
{
    public DocumentVersionDto()
    {
    }

    public DocumentVersionDto(string documentId, long versionId)
    {
        DocumentId = documentId;
        VersionId = versionId;
    }

    [JsonPropertyName("documentId")]
    [Required]
    public string DocumentId { get; set; }

    [JsonPropertyName("versionId")]
    [RequiredLongGreaterThanZero]
    public long VersionId { get; set; }
}