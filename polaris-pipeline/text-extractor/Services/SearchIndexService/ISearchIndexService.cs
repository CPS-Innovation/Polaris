using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Domain.SearchIndex;
using Common.Dto.Response;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace text_extractor.Services.CaseSearchService
{
    public interface ISearchIndexService
    {
        Task<int> SendStoreResultsAsync(AnalyzeResults analyzeResults, string documentId, long cmsCaseId, string cmsDocumentId, long versionId, string blobName, Guid correlationId);

        Task<IList<StreamlinedSearchLine>> QueryAsync(long caseId, string searchTerm);

        Task<IndexDocumentsDeletedResult> RemoveCaseIndexEntriesAsync(long caseId, Guid correlationId);

        Task<SearchIndexCountResult> GetCaseIndexCount(long caseId, Guid correlationId);

        Task<SearchIndexCountResult> GetDocumentIndexCount(long caseId, string documentId, long versionId, Guid correlationId);
    }
}