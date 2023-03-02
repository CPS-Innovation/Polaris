using System;
using System.Collections.Generic;
using Common.Domain.SearchIndex;
using Common.Logging;
using Common.Mappers.Contracts;
using Microsoft.Extensions.Logging;

namespace Common.Mappers
{
    public class StreamlinedSearchLineMapper : IStreamlinedSearchLineMapper
    {
        private readonly ILogger<StreamlinedSearchLineMapper> _logger;

        public StreamlinedSearchLineMapper(ILogger<StreamlinedSearchLineMapper> logger)
        {
            _logger = logger;
        }
        
        public StreamlinedSearchLine Map(SearchLine searchLine, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(Map), string.Empty);
            
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

            _logger.LogMethodExit(correlationId, nameof(Map), string.Empty);
            return streamlinedSearchLine;
        }
    }
}
