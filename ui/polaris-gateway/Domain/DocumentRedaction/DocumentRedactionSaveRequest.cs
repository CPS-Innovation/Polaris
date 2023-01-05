using System.Collections.Generic;
using Newtonsoft.Json;

namespace RumpoleGateway.Domain.DocumentRedaction
{
    public class DocumentRedactionSaveRequest
    {
        [JsonProperty("docId")]
        public string DocId { get; set; }

        [JsonProperty("redactions")]
        public List<RedactionDefinition> Redactions { get; set; }
    }
}
