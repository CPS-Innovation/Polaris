using Microsoft.Extensions.Logging;
using polaris_common.Domain.SearchIndex;
using polaris_common.Logging;
using polaris_common.Mappers.Contracts;

namespace polaris_common.Mappers
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
