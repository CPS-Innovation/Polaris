using Common.Domain.SearchIndex;

namespace Common.Mappers.Contracts
{
    public interface IStreamlinedSearchLineMapper
    {
        StreamlinedSearchLine Map(SearchLine searchLine);
    }
}
