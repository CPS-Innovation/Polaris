using Common.Domain.Ocr;
using Common.Dto.Request.Redaction;
using System.Collections.Generic;
using System.Linq;

namespace Common.Mappers;

public class RedactionSearchDtoMapper : IRedactionSearchDtoMapper
{
    public IEnumerable<RedactionSearchDto> Map(IEnumerable<ReadResult> readResults)
    {
        var redactionSearchDtos = new List<RedactionSearchDto>();
        foreach (var readResult in readResults)
        {
            var words = readResult.Lines.SelectMany(x => x.Words);
            redactionSearchDtos.AddRange(words.Select(word => new RedactionSearchDto
            {
                PageIndex = readResult.Page,
                Width = readResult.Width,
                Height = readResult.Height,
                RedactionCoordinates = new RedactionCoordinatesDto
                {
                    X1 = (double)word.BoundingBox[0]!, 
                    Y1 = (double)word.BoundingBox[1]!, 
                    X2 = (double)word.BoundingBox[2]!, 
                    Y2 = (double)word.BoundingBox[3]!
                },
                Word = word.Text
            }));
        }

        return redactionSearchDtos;
    }
}