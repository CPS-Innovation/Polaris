using System.Collections.Generic;
using System.Text.Json.Serialization;
using Common.Dto.Request.DocumentManipulation;

namespace Common.Dto.Request
{
    public class DocumentModificationRequestDto
    {
        [JsonPropertyName("documentModifications")]
        public List<DocumentModificationDto> DocumentModifications { get; set; }
    }
}