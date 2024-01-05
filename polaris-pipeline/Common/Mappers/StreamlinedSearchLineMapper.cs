using System;
using System.Collections.Generic;
using Common.Domain.SearchIndex;
using Common.Mappers.Contracts;

namespace Common.Mappers
{
    public class StreamlinedSearchLineMapper : IStreamlinedSearchLineMapper
    {
        public StreamlinedSearchLine Map(SearchLine searchLine, Guid correlationId)
        {

            var streamlinedSearchLine = new StreamlinedSearchLine
            {
                Text = searchLine.Text,
                Id = searchLine.Id,
                LineIndex = searchLine.LineIndex,
                PageIndex = searchLine.PageIndex,
                PageHeight = searchLine.PageHeight,
                PageWidth = searchLine.PageWidth,
                FileName = searchLine.FileName,
                Words = new List<StreamlinedWord>()
            };

            return streamlinedSearchLine;
        }
    }
}
