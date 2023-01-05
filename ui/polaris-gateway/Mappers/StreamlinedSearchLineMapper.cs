using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using PolarisGateway.Domain.Logging;
using PolarisGateway.Domain.PolarisPipeline;

namespace PolarisGateway.Mappers
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
                CaseId = searchLine.CaseId,
                DocumentId = searchLine.DocumentId,
                Id = searchLine.Id,
                LineIndex = searchLine.LineIndex,
                PageIndex = searchLine.PageIndex,
                PageHeight = searchLine.PageHeight,
                PageWidth = searchLine.PageWidth,
                FileName = searchLine.FileName,
                VersionId = searchLine.VersionId,
                Words = new List<StreamlinedWord>()
            };

            _logger.LogMethodExit(correlationId, nameof(Map), string.Empty);
            return streamlinedSearchLine;
        }
    }
}
