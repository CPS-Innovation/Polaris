using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Services.ClearDownService
{
    public interface IClearDownService
    {
        Task DeleteCaseAsync(IDurableOrchestrationClient client, string caseUrn, int caseId, Guid correlationId);
    }
}
