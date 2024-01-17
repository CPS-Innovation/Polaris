using Common.Domain.SearchIndex;
using Common.Factories.Contracts;
using Common.Mappers.Contracts;

namespace Common.Factories
{
    public class StreamlinedSearchResultFactory : IStreamlinedSearchResultFactory
    {
        private readonly IStreamlinedSearchLineMapper _streamlinedSearchLineMapper;
        private readonly IStreamlinedSearchWordMapper _streamlinedSearchWordMapper;

        public StreamlinedSearchResultFactory(IStreamlinedSearchLineMapper streamlinedSearchLineMapper, IStreamlinedSearchWordMapper streamlinedSearchWordMapper)
        {
            _streamlinedSearchLineMapper = streamlinedSearchLineMapper;
            _streamlinedSearchWordMapper = streamlinedSearchWordMapper;
        }

        public StreamlinedSearchLine Create(SearchLine searchLine, string searchTerm)
        {
            var streamlinedSearchLine = _streamlinedSearchLineMapper.Map(searchLine);
            foreach (var word in searchLine.Words)
            {
                streamlinedSearchLine.Words.Add(_streamlinedSearchWordMapper.Map(word, searchTerm));
            }

            return streamlinedSearchLine;
        }
    }
}
