using System.Collections.Generic;
using System.Text.Json.Serialization;
using Common.Dto.Request.DocumentManipulation;
using Newtonsoft.Json;

namespace Common.Dto.Request
{
    public class DocumentModificationRequestDto
    {
        [JsonProperty("documentModifications")]
        [JsonPropertyName("documentModifications")]
        public List<DocumentModificationDto> DocumentModifications { get; set; }
    }
}