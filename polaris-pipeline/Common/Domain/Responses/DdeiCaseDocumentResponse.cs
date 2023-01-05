using Newtonsoft.Json;

namespace Common.Domain.Responses;

public class DdeiCaseDocumentResponse
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("versionId")]
    public long VersionId { get; set; }

    [JsonProperty("originalFileName")]
    public string OriginalFileName { get; set; }

    [JsonProperty("mimeType")]
    public string MimeType { get; set; }
    
    [JsonProperty("cmsDocCategory")] 
    public string CmsDocCategory { get; set; }
    
    [JsonProperty("typeId")] 
    public string DocumentTypeId { get; set; }
    
    [JsonProperty("type")] 
    public string DocumentType { get; set; }

    [JsonProperty("date")]
    public string DocumentDate { get; set; }
}
