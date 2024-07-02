using System.Collections.Generic;
using Common.Dto.Request.DocumentManipulation;
using Newtonsoft.Json;

namespace Common.Dto.Request
{
    public class DocumentModificationRequestDto
    {
        [JsonProperty("documentChanges")]
        public List<DocumentChangesDto> DocumentChanges { get; set; }
    }
}