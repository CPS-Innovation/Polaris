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
            foreach (var word in readResult.Lines.SelectMany(l => l?.Words ?? Enumerable.Empty<Word>()))
            {
                var bb = word?.BoundingBox;
                if (bb == null || bb.Count < 8 || bb.Any(v => v == null))
                {
                    continue;
                }

                var xCoords = new[] { bb[0]!.Value, bb[2]!.Value, bb[4]!.Value, bb[6]!.Value };
                var yCoords = new[] { bb[1]!.Value, bb[3]!.Value, bb[5]!.Value, bb[7]!.Value };

                redactionSearchDtos.Add(new RedactionSearchDto
                {
                    PageIndex = readResult.Page,
                    Width = readResult.Width,
                    Height = readResult.Height,
                    Word = word.Text,
                    RedactionCoordinates = new RedactionCoordinatesDto
                    {
                        X1 = xCoords.Min(),
                        Y1 = yCoords.Min(),
                        X2 = xCoords.Max(),
                        Y2 = yCoords.Max()
                    }
                });
            }
        }

        return redactionSearchDtos;
    }
}