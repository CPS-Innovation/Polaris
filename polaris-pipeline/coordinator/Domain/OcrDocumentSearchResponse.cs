using System.Collections.Generic;
using Common.Dto.Request.Redaction;

namespace coordinator.Domain;

public class OcrDocumentSearchResponse
{
    public IEnumerable<RedactionDefinitionDto> redactionDefinitionDtos { get; set; }

    public string FailureReason { get; set; }
}