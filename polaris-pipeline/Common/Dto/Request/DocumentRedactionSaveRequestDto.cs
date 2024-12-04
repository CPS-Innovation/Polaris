using System.Collections.Generic;
using System.Text.Json.Serialization;
using Common.Dto.Request.DocumentManipulation;
using Common.Dto.Request.Redaction;

namespace Common.Dto.Request
{
    public class DocumentRedactionSaveRequestDto
    {
        [JsonPropertyName("redactions")]
        public List<RedactionDefinitionDto> Redactions { get; set; }

        [JsonPropertyName("documentModifications")]
        public List<DocumentModificationDto> DocumentModifications { get; set; }
    }
}
