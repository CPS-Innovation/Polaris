using Common.Domain.SearchIndex;

namespace text_extractor.Mappers.Contracts
{
    public interface IStreamlinedSearchLineMapper
    {
        StreamlinedSearchLine Map(SearchLine searchLine);
    }
}
