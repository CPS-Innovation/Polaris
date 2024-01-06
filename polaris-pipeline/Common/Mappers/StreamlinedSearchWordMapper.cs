using System;
using System.Text.RegularExpressions;
using Common.Domain.SearchIndex;
using Common.Mappers.Contracts;
using FuzzySharp;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Common.Mappers
{
    public class StreamlinedSearchWordMapper : IStreamlinedSearchWordMapper
    {
        public StreamlinedSearchWordMapper()
        {
        }

        public StreamlinedWord Map(Word word, string searchTerm, Guid correlationId)
        {
            var searchTermLookup = SearchTermIncluded(word.Text, searchTerm, correlationId);
            var result = new StreamlinedWord
            {
                Text = word.Text,
                BoundingBox = searchTermLookup.TermFound ? word.BoundingBox : null,
                StreamlinedMatchType = searchTermLookup.SearchMatchType
            };

            return result;
        }

        private SearchTermResult SearchTermIncluded(string wordText, string searchTerm, Guid correlationId)
        {
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

            return new SearchTermResult(false, StreamlinedMatchType.None);
        }
    }
}
