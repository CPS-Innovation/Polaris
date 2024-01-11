using Common.Domain.SearchIndex;

namespace Common.Factories.Contracts
{
    public interface IStreamlinedSearchResultFactory
    {
        StreamlinedSearchLine Create(SearchLine searchLine, string searchTerm);
    }
}
