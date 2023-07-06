using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Domain.Entity;
using Common.Domain.SearchIndex;
using Common.ValueObjects;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Common.Services.CaseSearchService.Contracts
{
    public interface ICaseSearchClient
    {
        Task SendStoreResultsAsync(AnalyzeResults analyzeResults, PolarisDocumentId polarisDocumentId, long cmsCaseId, string cmsDocumentId, long versionId, string blobPath, Guid correlationId);

        Task<bool> WaitForStoreResultsAsync(AnalyzeResults analyzeResults, long cmsCaseId, string cmsDocumentId, long versionId, Guid correlationId);

        Task<bool> WaitForCaseEmptyResultsAsync(long cmsCaseId, Guid correlationId);

        Task<IList<StreamlinedSearchLine>> QueryAsync(int caseId, List<BaseDocumentEntity> documents, string searchTerm, Guid correlationId);

        Task RemoveCaseIndexEntriesAsync(long caseId, Guid correlationId);

        Task RemoveResultsByBlobNameAsync(long cmsCaseId, string blobName, Guid correlationId);
    }
}