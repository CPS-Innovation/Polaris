using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Services.CleardownService
{
    public interface ICleardownService
    {
        Task DeleteCaseAsync(IDurableOrchestrationClient client, string caseUrn, int caseId, Guid correlationId, bool waitForIndexToSettle);
    }
}
