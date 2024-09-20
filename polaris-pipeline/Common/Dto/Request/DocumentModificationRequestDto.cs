using System.Collections.Generic;
using Common.Dto.Request.DocumentManipulation;
using Newtonsoft.Json;

namespace Common.Dto.Request
{
    public class DocumentModificationRequestDto
    {
        [JsonProperty("documentModifications")]
        public List<DocumentModificationDto> DocumentModifications { get; set; }
    }
}