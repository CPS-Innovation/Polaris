using polaris_common.Domain.SearchIndex;

namespace polaris_common.Factories.Contracts
{
    public interface IPipelineClientSearchRequestFactory
    {
        HttpRequestMessage Create(long cmsCaseId, string searchTerm, Guid correlationId, IEnumerable<SearchFilterDocument> documents);
    }
}