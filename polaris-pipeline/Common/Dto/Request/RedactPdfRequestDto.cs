using System.Collections.Generic;
using Common.Dto.Request.Redaction;

namespace Common.Dto.Request
{
    public class RedactPdfRequestDto
    {
        public long CaseId { get; set; }

        public string DocumentId { get; set; }

        public long VersionId { get; set; }

        public string FileName { get; set; }

        public List<RedactionDefinitionDto> RedactionDefinitions { get; set; }
    }
}
