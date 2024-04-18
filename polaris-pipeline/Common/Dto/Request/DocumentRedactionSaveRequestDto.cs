using System.Collections.Generic;
using Common.Dto.Request.Redaction;
using Newtonsoft.Json;

namespace Common.Dto.Request
{
    public class DocumentRedactionSaveRequestDto
    {
        [JsonProperty("redactions")]
        public List<RedactionDefinitionDto> Redactions { get; set; }
    }
}
