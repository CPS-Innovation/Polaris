﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Domain.SearchIndex;
using Common.ValueObjects;
using Common.Dto.Response;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace text_extractor.Services.CaseSearchService
{
    public interface ISearchIndexService
    {
        Task SendStoreResultsAsync(AnalyzeResults analyzeResults, PolarisDocumentId polarisDocumentId, long cmsCaseId, string cmsDocumentId, long versionId, string blobName, Guid correlationId);

        Task<IndexSettledResult> WaitForStoreResultsAsync(long cmsCaseId, string cmsDocumentId, long versionId, long targetCount);

        Task<IndexSettledResult> WaitForCaseEmptyResultsAsync(long cmsCaseId);

        Task<IList<StreamlinedSearchLine>> QueryAsync(long caseId, List<SearchFilterDocument> documents, string searchTerm);

        IList<StreamlinedSearchLine> BuildStreamlinedResults(IList<SearchLine> searchResults, string searchTerm);

        Task<IndexDocumentsDeletedResult> RemoveCaseIndexEntriesAsync(long caseId);

        Task<SearchIndexCountResult> GetCaseIndexCount(long caseId);

        Task<SearchIndexCountResult> GetDocumentIndexCount(long caseId, string documentId, long versionId);
    }
}