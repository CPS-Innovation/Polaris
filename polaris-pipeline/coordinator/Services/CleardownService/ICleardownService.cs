using System;
using System.Threading.Tasks;

namespace coordinator.Services.ClearDownService
{
    public interface IClearDownService
    {
        Task DeleteCaseAsync(IDurableOrchestrationClient client, string caseUrn, int caseId, Guid correlationId);
    }
}
