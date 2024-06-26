using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Common.Domain.SearchIndex;
using Common.Dto.Response;
using Common.ValueObjects;

namespace coordinator.Clients.TextExtractor
{
        public interface ITextExtractorClient
        {
                Task<StoreCaseIndexesResult> StoreCaseIndexesAsync(PolarisDocumentId polarisDocumentId, string cmsCaseUrn, long cmsCaseId, string cmsDocumentId, long versionId, string blobName, Guid correlationId, Stream ocrResults);
                Task<IList<StreamlinedSearchLine>> SearchTextAsync(string caseUrn, long cmsCaseId, string searchTerm, Guid correlationId);
                Task<IndexDocumentsDeletedResult> RemoveCaseIndexesAsync(string caseUrn, long cmsCaseId, Guid correlationId);
                Task<SearchIndexCountResult> GetCaseIndexCount(string caseUrn, long cmsCaseId, Guid correlationId);
                Task<SearchIndexCountResult> GetDocumentIndexCount(string caseUrn, long cmsCaseId, string cmsDocumentId, long versionId, Guid correlationId);
        }
}