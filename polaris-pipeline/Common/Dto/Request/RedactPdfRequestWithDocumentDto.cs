using System.Collections.Generic;
using Common.Dto.Request.Redaction;

namespace Common.Dto.Request
{
    public class RedactPdfRequestWithDocumentDto
    {
        public long VersionId { get; set; }
        public string FileName { get; set; }

        public List<RedactionDefinitionDto> RedactionDefinitions { get; set; }
        public string Document { get; set; }
    }
}
