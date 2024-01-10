using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using polaris_common.Validators;

namespace polaris_common.Dto.Document;

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