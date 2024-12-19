using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Common.Validators;
using Newtonsoft.Json;

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

    [JsonProperty("documentId")]
    [JsonPropertyName("documentId")]
    [Required]
    public string DocumentId { get; set; }

    [JsonProperty("versionId")]
    [JsonPropertyName("versionId")]
    [RequiredLongGreaterThanZero]
    public long VersionId { get; set; }
}