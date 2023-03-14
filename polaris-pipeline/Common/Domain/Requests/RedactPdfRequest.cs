using System.Collections.Generic;
using Common.Domain.Redaction;

namespace Common.Domain.Requests
{
    public class RedactPdfRequest
    {
        public long CaseId { get; set; }

        public string DocumentId { get; set; }
        
        public long VersionId { get; set; }

        public string FileName { get; set; }

        public List<RedactionDefinition> RedactionDefinitions { get; set; }
    }
}
