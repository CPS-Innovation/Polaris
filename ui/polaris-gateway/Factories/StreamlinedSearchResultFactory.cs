using System;
using Microsoft.Extensions.Logging;
using PolarisGateway.Domain.Logging;
using PolarisGateway.Domain.PolarisPipeline;
using PolarisGateway.Mappers;

namespace PolarisGateway.Factories
{
    public class StreamlinedSearchResultFactory : IStreamlinedSearchResultFactory
    {
        private readonly IStreamlinedSearchLineMapper _streamlinedSearchLineMapper;
        private readonly IStreamlinedSearchWordMapper _streamlinedSearchWordMapper;
        private readonly ILogger<StreamlinedSearchResultFactory> _logger;

        public StreamlinedSearchResultFactory(IStreamlinedSearchLineMapper streamlinedSearchLineMapper, IStreamlinedSearchWordMapper streamlinedSearchWordMapper, ILogger<StreamlinedSearchResultFactory> logger)
        {
            _streamlinedSearchLineMapper = streamlinedSearchLineMapper;
            _streamlinedSearchWordMapper = streamlinedSearchWordMapper;
            _logger = logger;
        } 

        public StreamlinedSearchLine Create(SearchLine searchLine, string searchTerm, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(Create), string.Empty);
            var streamlinedSearchLine = _streamlinedSearchLineMapper.Map(searchLine, correlationId);
            foreach (var word in searchLine.Words)
            {
                streamlinedSearchLine.Words.Add(_streamlinedSearchWordMapper.Map(word, searchTerm, correlationId));
            }

            _logger.LogMethodExit(correlationId, nameof(Create), string.Empty);
            return streamlinedSearchLine;
        }
    }
}
