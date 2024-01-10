using polaris_common.Domain.SearchIndex;
using polaris_common.ValueObjects;

namespace polaris_common.Clients.Contracts
{
    public interface ITextExtractorClient
    {
        Task ExtractTextAsync(PolarisDocumentId polarisDocumentId, long cmsCaseId, string cmsDocumentId, long versionId, string blobName, Guid correlationId, Stream documentStream);
        Task<IList<StreamlinedSearchLine>> SearchTextAsync(long cmsCaseId, string searchTerm, Guid correlationId, IEnumerable<SearchFilterDocument> documents);
    }
}