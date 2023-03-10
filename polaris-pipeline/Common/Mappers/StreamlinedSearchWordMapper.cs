using System;
using System.Text.RegularExpressions;
using Common.Domain.Extensions;
using Common.Domain.SearchIndex;
using Common.Logging;
using Common.Mappers.Contracts;
using FuzzySharp;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;

namespace Common.Mappers
{
    public class StreamlinedSearchWordMapper : IStreamlinedSearchWordMapper
    {
        private readonly ILogger<StreamlinedSearchWordMapper> _logger;

        public StreamlinedSearchWordMapper(ILogger<StreamlinedSearchWordMapper> logger)
        {
            _logger = logger;
        }

        public StreamlinedWord Map(Word word, string searchTerm, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(Map), $"Word object: {word.ToJson()}, Search Term: {searchTerm}");
            
            var searchTermLookup = SearchTermIncluded(word.Text, searchTerm, correlationId);
            var result = new StreamlinedWord
            {
                Text = word.Text,
                BoundingBox = searchTermLookup.TermFound ? word.BoundingBox : null,
                StreamlinedMatchType = searchTermLookup.SearchMatchType
            };

            _logger.LogMethodExit(correlationId, nameof(Map), result.ToJson());
            return result;
        }

        private SearchTermResult SearchTermIncluded(string wordText, string searchTerm, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(SearchTermIncluded), $"Word Text: {wordText}, Search Term: {searchTerm}");
            
            var tidiedText = wordText.Replace(" ", "");
            if (searchTerm.Equals(tidiedText, StringComparison.CurrentCultureIgnoreCase))
                return new SearchTermResult(true, StreamlinedMatchType.Exact);

            var partialWeighting = Fuzz.PartialRatio(tidiedText, searchTerm);
            if (partialWeighting >= 95)
            {
                return Regex.IsMatch(wordText, @"\b" + searchTerm + @"\b", RegexOptions.IgnoreCase)
                    ? new SearchTermResult(true, StreamlinedMatchType.Exact)
                    //: new SearchTermResult(true, StreamlinedMatchType.Fuzzy); commented out fuzzy matches for now until we support them when searching the index
                    : new SearchTermResult(false, StreamlinedMatchType.None);
            }

            var result = new SearchTermResult(false, StreamlinedMatchType.None);
            _logger.LogMethodExit(correlationId, nameof(SearchTermIncluded), $"result: {result.ToJson()}");
            return result;
        }
    }
}
