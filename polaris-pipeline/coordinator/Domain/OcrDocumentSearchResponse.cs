using System.Collections.Generic;
using Common.Dto.Request.Redaction;

namespace coordinator.Domain;

public class OcrDocumentSearchResponse
{
    public IEnumerable<RedactionDefinitionDto> RedactionDefinitionDtos { get; set; }

    public string FailureReason { get; set; }
}