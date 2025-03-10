using System.Collections.Generic;
using Common.Dto.Request.Redaction;

namespace Common.Dto.Request
{
    public class RedactPdfRequestWithDocumentDto
    {
        public List<RedactionDefinitionDto> RedactionDefinitions { get; set; }

        public string Document { get; set; }
    }
}
