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
                Task<StoreCaseIndexesResult> StoreCaseIndexesAsync(string documentId, string urn, int caseId, long versionId, Guid correlationId, Stream ocrResults);
                Task<IList<StreamlinedSearchLine>> SearchTextAsync(string urn, int caseId, string searchTerm, Guid correlationId);
                Task<IndexDocumentsDeletedResult> RemoveCaseIndexesAsync(string urn, int caseId, Guid correlationId);
                Task<SearchIndexCountResult> GetCaseIndexCount(string urn, int caseId, Guid correlationId);
                Task<SearchIndexCountResult> GetDocumentIndexCount(string urn, int caseId, string documentId, long versionId, Guid correlationId);
        }
}