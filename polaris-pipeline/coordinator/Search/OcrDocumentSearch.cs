// <copyright file="OcrDocumentSearch.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Search;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Common.Domain.Ocr;
using Common.Dto.Request.Redaction;
using Common.Mappers;
using coordinator.Domain;

public class OcrDocumentSearch(IRedactionSearchDtoMapper redactionSearchDtoMapper): IOcrDocumentSearch
{
    public OcrDocumentSearchResponse Search(string searchText, AnalyzeResults results)
    {
        var ocrDocumentSearchResponse = new OcrDocumentSearchResponse();
        ocrDocumentSearchResponse.RedactionDefinitionDtos = new List<RedactionDefinitionDto>();

        try
        {
            var searchTermList = GetSearchTerms(searchText);
            if (searchTermList.Count == 0)
            {
                return ocrDocumentSearchResponse;
            }

            var redactionSearchDtos = redactionSearchDtoMapper.Map(results.ReadResults).ToList();
            var toBeRedacted = FindMatches(searchTermList, redactionSearchDtos);
            ocrDocumentSearchResponse.RedactionDefinitionDtos = BuildRedactionDefinitions(toBeRedacted);
            return ocrDocumentSearchResponse;
        }
        catch (Exception ex)
        {
            ocrDocumentSearchResponse.FailureReason = ex.Message;
            return ocrDocumentSearchResponse;
        }
    }

    private static List<string> GetSearchTerms(string searchText)
    {
        return searchText
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    private static List<RedactionSearchDto> FindMatches(IReadOnlyList<string> searchTermList, IReadOnlyList<RedactionSearchDto> redactionSearchDtos)
    {
        if (searchTermList.Count == 1)
        {
            return FindSingleTermMatches(searchTermList[0], redactionSearchDtos);
        }

        return FindPhraseMatches(searchTermList, redactionSearchDtos);
    }

    private static List<RedactionSearchDto> FindSingleTermMatches(string searchTerm, IReadOnlyList<RedactionSearchDto> redactionSearchDtos)
    {
        var matches = new List<RedactionSearchDto>();
        foreach (var redactionSearchDto in redactionSearchDtos)
        {
            matches.AddRange(GetTermMatches(redactionSearchDto, searchTerm));
        }

        return matches;
    }

    private static List<RedactionSearchDto> FindPhraseMatches(IReadOnlyList<string> searchTermList, IReadOnlyList<RedactionSearchDto> redactionSearchDtos)
    {
        var toBeRedacted = new List<RedactionSearchDto>();

        for (var i = 0; i < redactionSearchDtos.Count; i++)
        {
            if (i + searchTermList.Count > redactionSearchDtos.Count)
            {
                continue;
            }

            var firstTermMatches = GetTermMatches(redactionSearchDtos[i], searchTermList[0]);
            foreach (var firstTermMatch in firstTermMatches)
            {
                var phraseMatch = BuildPhraseMatch(searchTermList, redactionSearchDtos, i, firstTermMatch);
                if (phraseMatch.Count > 0)
                {
                    toBeRedacted.AddRange(phraseMatch);
                }
            }
        }

        return toBeRedacted;
    }

    private static List<RedactionSearchDto> BuildPhraseMatch(IReadOnlyList<string> searchTermList, IReadOnlyList<RedactionSearchDto> redactionSearchDtos, int startIndex, RedactionSearchDto firstTermMatch)
    {
        var potentialRedactions = new List<RedactionSearchDto>(searchTermList.Count) { firstTermMatch };

        for (var j = 1; j < searchTermList.Count; j++)
        {
            var nextTermMatch = GetFirstTermMatch(redactionSearchDtos[startIndex + j], searchTermList[j]);
            if (nextTermMatch == null)
            {
                return [];
            }

            potentialRedactions.Add(nextTermMatch);
        }

        return potentialRedactions;
    }

    private static List<RedactionDefinitionDto> BuildRedactionDefinitions(IReadOnlyCollection<RedactionSearchDto> toBeRedacted)
    {
        return toBeRedacted
            .Select(x => x.PageIndex)
            .Distinct()
            .Select(pageIndex => new { pageIndex, page = toBeRedacted.First(x => x.PageIndex == pageIndex) })
            .Select(@t => new RedactionDefinitionDto
            {
                PageIndex = @t.pageIndex,
                Width = @t.page.Width,
                Height = @t.page.Height,
                RedactionCoordinates = toBeRedacted.Where(x => x.PageIndex == @t.pageIndex)
                    .Select(x => x.RedactionCoordinates)
                    .ToList(),
            })
            .ToList();
    }

    private static List<RedactionSearchDto> GetTermMatches(RedactionSearchDto redactionSearchDto, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(redactionSearchDto.Word) || string.IsNullOrWhiteSpace(searchTerm))
        {
            return new List<RedactionSearchDto>();
        }

        var matches = Regex.Matches(
            redactionSearchDto.Word,
            $@"(?<![\p{{L}}\p{{N}}]){Regex.Escape(searchTerm)}(?![\p{{L}}\p{{N}}])",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        if (matches.Count == 0)
        {
            return new List<RedactionSearchDto>();
        }

        var redactionSearchResults = new List<RedactionSearchDto>(matches.Count);
        foreach (Match match in matches)
        {
            redactionSearchResults.Add(CreateMatchRedaction(redactionSearchDto, match.Index, match.Length));
        }

        return redactionSearchResults;
    }

    private static RedactionSearchDto GetFirstTermMatch(RedactionSearchDto redactionSearchDto, string searchTerm)
    {
        return GetTermMatches(redactionSearchDto, searchTerm).FirstOrDefault();
    }

    private static RedactionSearchDto CreateMatchRedaction(RedactionSearchDto source, int startIndex, int length)
    {
        var sourceWordLength = source.Word.Length;
        var sourceCoordinates = source.RedactionCoordinates;
        var x1 = sourceCoordinates.X1;
        var x2 = sourceCoordinates.X2;
        var y1 = sourceCoordinates.Y1;
        var y2 = sourceCoordinates.Y2;

        if (sourceWordLength == 0 || (startIndex == 0 && length == sourceWordLength))
        {
            return new RedactionSearchDto
            {
                PageIndex = source.PageIndex,
                Width = source.Width,
                Height = source.Height,
                Word = source.Word,
                RedactionCoordinates = new RedactionCoordinatesDto
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                },
            };
        }

        var width = x2 - x1;
        var charWidth = width / sourceWordLength;
        var matchStartRatio = (double)startIndex / sourceWordLength;
        var matchEndRatio = (double)(startIndex + length) / sourceWordLength;
        var adjustedX1 = Math.Max(x1, (x1 + (width * matchStartRatio)) - (charWidth / 2));
        var adjustedX2 = Math.Min(x2, (x1 + (width * matchEndRatio)) + (charWidth / 2));

        return new RedactionSearchDto
        {
            PageIndex = source.PageIndex,
            Width = source.Width,
            Height = source.Height,
            Word = source.Word,
            RedactionCoordinates = new RedactionCoordinatesDto
            {
                X1 = adjustedX1,
                Y1 = y1,
                X2 = adjustedX2,
                Y2 = y2,
            },
        };
    }
}
