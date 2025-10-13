using System.Collections.Generic;
using Common.Domain.Ocr;
using Common.Dto.Request.Redaction;

namespace Common.Mappers;

public interface IRedactionSearchDtoMapper
{
    public IEnumerable<RedactionSearchDto> Map(IEnumerable<ReadResult> readResults);
}