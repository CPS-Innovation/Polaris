using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Common.Domain.SearchIndex;
using Common.Dto.Response;

namespace coordinator.Clients.TextExtractor
{
        public interface ITextExtractorClient
        {
                Task<StoreCaseIndexesResult> StoreCaseIndexesAsync(string documentId, string urn, long caseId, long versionId, string blobName, Guid correlationId, Stream ocrResults);
                Task<IList<StreamlinedSearchLine>> SearchTextAsync(string urn, long caseId, string searchTerm, Guid correlationId);
                Task<IndexDocumentsDeletedResult> RemoveCaseIndexesAsync(string urn, long caseId, Guid correlationId);
                Task<SearchIndexCountResult> GetCaseIndexCount(string urn, long caseId, Guid correlationId);
                Task<SearchIndexCountResult> GetDocumentIndexCount(string urn, long caseId, string documentId, long versionId, Guid correlationId);
        }
}