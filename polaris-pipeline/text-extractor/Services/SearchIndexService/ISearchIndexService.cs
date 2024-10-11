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
        Task<int> SendStoreResultsAsync(AnalyzeResults analyzeResults, string documentId, int caseId, long versionId, string blobName, Guid correlationId);

        Task<IList<StreamlinedSearchLine>> QueryAsync(int caseId, string searchTerm);

        Task<IndexDocumentsDeletedResult> RemoveCaseIndexEntriesAsync(int caseId, Guid correlationId);

        Task<SearchIndexCountResult> GetCaseIndexCount(int caseId, Guid correlationId);

        Task<SearchIndexCountResult> GetDocumentIndexCount(int caseId, string documentId, long versionId, Guid correlationId);
    }
}