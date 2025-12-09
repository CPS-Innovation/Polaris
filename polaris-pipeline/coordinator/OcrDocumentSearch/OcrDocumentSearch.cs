using Common.Domain.Ocr;
using Common.Dto.Request.Redaction;
using Common.Mappers;
using coordinator.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace coordinator.OcrDocumentSearch;

public class OcrDocumentSearch : IOcrDocumentSearch
{
    private readonly IRedactionSearchDtoMapper _redactionSearchDtoMapper;

    public OcrDocumentSearch(IRedactionSearchDtoMapper redactionSearchDtoMapper)
    {
        _redactionSearchDtoMapper = redactionSearchDtoMapper;
    }

    public OcrDocumentSearchResponse Search(string searchText, AnalyzeResults results)
    {
        var ocrDocumentSearchResponse = new OcrDocumentSearchResponse();
        var redactionDefinitionDtos = new List<RedactionDefinitionDto>();
        var searchTermList = searchText.Split(' ').ToList();
        try
        {
            var toBeRedacted = new List<RedactionSearchDto>();
            var redactionSearchDtos = _redactionSearchDtoMapper.Map(results.ReadResults).ToList();

            for (int i = 0; i < redactionSearchDtos.Count; i++)
            {
                if (!redactionSearchDtos[i].Word
                        .Contains(searchTermList[0], StringComparison.InvariantCultureIgnoreCase)) continue;

                var potentialRedactions = new List<RedactionSearchDto>(searchTermList.Count) { redactionSearchDtos[i] };
                for (int j = 1; j < searchTermList.Count; j++)
                {
                    if (redactionSearchDtos[i + j].Word
                        .Contains(searchTermList[j], StringComparison.InvariantCultureIgnoreCase))
                    {
                        potentialRedactions.Add(redactionSearchDtos[i + j]);
                        continue;
                    }

                    break;
                }

                if (searchTermList.Count != potentialRedactions.Count) continue;

                toBeRedacted.AddRange(potentialRedactions);
                i += searchTermList.Count;
            }

            var pageIndexes = toBeRedacted.Select(x => x.PageIndex).Distinct();

            redactionDefinitionDtos.AddRange(pageIndexes
                .Select(pageIndex => new { pageIndex, page = toBeRedacted.First(x => x.PageIndex == pageIndex) })
                .Select(@t => new RedactionDefinitionDto
                {
                    PageIndex = @t.pageIndex,
                    Width = @t.page.Width,
                    Height = @t.page.Height,
                    RedactionCoordinates = toBeRedacted.Where(x => x.PageIndex == @t.pageIndex)
                        .Select(x => x.RedactionCoordinates)
                        .ToList()
                }));

            ocrDocumentSearchResponse.redactionDefinitionDtos = redactionDefinitionDtos;
            return ocrDocumentSearchResponse;
        }
        catch (Exception ex)
        {
            ocrDocumentSearchResponse.FailureReason = ex.Message;
            return ocrDocumentSearchResponse;
        }
    }
}