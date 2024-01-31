using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Common.Domain.SearchIndex;
using Common.Dto.Response;
using Common.ValueObjects;

namespace coordinator.Clients.Contracts
{
    public interface ITextExtractorClient
    {
        Task ExtractTextAsync(PolarisDocumentId polarisDocumentId, string cmsCaseUrn, long cmsCaseId, string cmsDocumentId, long versionId, string blobName, Guid correlationId, Stream documentStream);
        Task<IList<StreamlinedSearchLine>> SearchTextAsync(long cmsCaseId, string searchTerm, Guid correlationId, IEnumerable<SearchFilterDocument> documents);
        Task<IndexDocumentsDeletedResult> RemoveCaseIndexesAsync(long cmsCaseId, Guid correlationId);
        Task<IndexSettledResult> WaitForCaseEmptyResultsAsync(long cmsCaseId, Guid correlationId);
    }
}