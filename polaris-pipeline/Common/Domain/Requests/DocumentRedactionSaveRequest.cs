using System.Collections.Generic;
using Common.Domain.Redaction;
using Newtonsoft.Json;

namespace Common.Domain.Requests
{
    public class DocumentRedactionSaveRequest
    {
        [JsonProperty("docId")]
        public string DocId { get; set; }

        [JsonProperty("redactions")]
        public List<RedactionDefinition> Redactions { get; set; }
    }
}
