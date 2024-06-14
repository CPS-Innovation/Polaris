using System;
using System.Threading.Tasks;
using Common.Dto.Response;

namespace coordinator.Services.TextExtractService
{
    [Obsolete("The client should poll for the desired number of records")]
    public interface ITextExtractService
    {
        Task<IndexSettledResult> WaitForCaseEmptyResultsAsync(string caseUrn, long cmsCaseId, Guid correlationId);
    }
}