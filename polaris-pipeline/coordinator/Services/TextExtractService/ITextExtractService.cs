using System;
using System.Threading.Tasks;
using Common.Dto.Response;

namespace coordinator.Services.TextExtractService
{
    public interface ITextExtractService
    {
        Task<IndexSettledResult> WaitForDocumentStoreResultsAsync(string caseUrn, long cmsCaseId, string cmsDocumentId, long versionId, long targetCount, Guid correlationId);
        Task<IndexSettledResult> WaitForCaseEmptyResultsAsync(string caseUrn, long cmsCaseId, Guid correlationId);
    }
}