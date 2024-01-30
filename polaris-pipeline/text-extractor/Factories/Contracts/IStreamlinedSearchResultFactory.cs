using Common.Domain.SearchIndex;

namespace text_extractor.Factories.Contracts
{
    public interface IStreamlinedSearchResultFactory
    {
        StreamlinedSearchLine Create(SearchLine searchLine, string searchTerm);
    }
}
