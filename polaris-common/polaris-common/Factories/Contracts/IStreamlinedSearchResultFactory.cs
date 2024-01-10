using polaris_common.Domain.SearchIndex;

namespace polaris_common.Factories.Contracts
{
    public interface IStreamlinedSearchResultFactory
    {
        StreamlinedSearchLine Create(SearchLine searchLine, string searchTerm, Guid correlationId);
    }
}
