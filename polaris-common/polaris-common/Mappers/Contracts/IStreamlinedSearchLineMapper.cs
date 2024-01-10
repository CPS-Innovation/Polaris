using polaris_common.Domain.SearchIndex;

namespace polaris_common.Mappers.Contracts
{
    public interface IStreamlinedSearchLineMapper
    {
        StreamlinedSearchLine Map(SearchLine searchLine, Guid correlationId);
    }
}
