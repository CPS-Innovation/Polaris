// <copyright file="OcrDocumentSearch.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Search;

using Common.Domain.Ocr;
using Common.Dto.Request.Redaction;
using Common.Mappers;
using coordinator.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class OcrDocumentSearch(IRedactionSearchDtoMapper redactionSearchDtoMapper) : IOcrDocumentSearch
{
    public OcrDocumentSearchResponse Search(string searchText, AnalyzeResults results)
    {
        var response = new OcrDocumentSearchResponse();

        try
        {
            var searchTerms = searchText
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            var redactionSearchDtos = redactionSearchDtoMapper
                .Map(results.ReadResults)
                .ToList();

            var toBeRedacted = new List<RedactionSearchDto>();

            for (int i = 0; i < redactionSearchDtos.Count; i++)
            {
                var matches = FindMatch(i, searchTerms, redactionSearchDtos);

                if (matches is not null)
                {
                    toBeRedacted.AddRange(matches);
                }
            }

            response.RedactionDefinitionDtos = BuildRedactionDefinitions(toBeRedacted);

            return response;
        }
        catch (Exception ex)
        {
            response.FailureReason = ex.Message;
            return response;
        }
    }

    private static List<RedactionSearchDto>? FindMatch(
        int startIndex,
        IReadOnlyList<string> searchTerms,
        IReadOnlyList<RedactionSearchDto> ocrWords)
    {
        var matches = new List<RedactionSearchDto>();

        for (int i = 0; i < searchTerms.Count; i++)
        {
            if (startIndex + i >= ocrWords.Count)
            {
                return null;
            }

            var coordinates = GetMatchingCoordinates(
                ocrWords[startIndex + i],
                searchTerms[i]);

            if (coordinates is null)
            {
                return null;
            }

            matches.Add(CloneDto(
                ocrWords[startIndex + i],
                coordinates));
        }

        return matches;
    }

    private static List<RedactionDefinitionDto> BuildRedactionDefinitions(
        List<RedactionSearchDto> matches)
    {
        return matches
            .GroupBy(x => x.PageIndex)
            .Select(group =>
            {
                var first = group.First();

                return new RedactionDefinitionDto
                {
                    PageIndex = group.Key,
                    Width = first.Width,
                    Height = first.Height,
                    RedactionCoordinates = group
                        .Select(x => x.RedactionCoordinates)
                        .ToList(),
                };
            })
            .ToList();
    }

    private static RedactionSearchDto CloneDto(
        RedactionSearchDto source,
        RedactionCoordinatesDto coordinates)
    {
        return new RedactionSearchDto
        {
            Word = source.Word,
            PageIndex = source.PageIndex,
            Width = source.Width,
            Height = source.Height,
            RedactionCoordinates = coordinates,
        };
    }

    private static RedactionCoordinatesDto? GetMatchingCoordinates(
        RedactionSearchDto dto,
        string searchTerm)
    {
        var match = FindExactTokenMatch(dto.Word, searchTerm);

        if (match is null)
        {
            return null;
        }

        var original = dto.RedactionCoordinates;

        var totalWidth = original.X2 - original.X1;
        var charWidth = totalWidth / dto.Word.Length;

        var x1 = original.X1 + (charWidth * match.Value.StartIndex);
        var x2 = x1 + (charWidth * match.Value.Length);

        return new RedactionCoordinatesDto
        {
            X1 = x1,
            Y1 = original.Y1,
            X2 = x2,
            Y2 = original.Y2,
        };
    }

    private static (int StartIndex, int Length)? FindExactTokenMatch(
        string input,
        string searchTerm)
    {
        var pattern =
            $@"(?<![\p{{L}}\p{{N}}]){Regex.Escape(searchTerm)}(?![\p{{L}}\p{{N}}])";

        var match = Regex.Match(
            input,
            pattern,
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        return match.Success
            ? (match.Index, match.Length)
            : null;
    }
}
