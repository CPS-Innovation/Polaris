using Newtonsoft.Json;
using polaris_common.Dto.Request.Redaction;

namespace polaris_common.Dto.Request
{
    public class DocumentRedactionSaveRequestDto
    {
        [JsonProperty("docId")]
        public string DocId { get; set; }

        [JsonProperty("redactions")]
        public List<RedactionDefinitionDto> Redactions { get; set; }
    }
}
