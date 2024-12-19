using System.Collections.Generic;
using System.Text.Json.Serialization;
using Common.Dto.Request.DocumentManipulation;
using Common.Dto.Request.Redaction;
using Newtonsoft.Json;

namespace Common.Dto.Request
{
    public class DocumentRedactionSaveRequestDto
    {
        [JsonProperty("redactions")]
        [JsonPropertyName("redactions")]
        public List<RedactionDefinitionDto> Redactions { get; set; }

        [JsonProperty("documentModifications")]
        [JsonPropertyName("documentModifications")]
        public List<DocumentModificationDto> DocumentModifications { get; set; }
    }
}
