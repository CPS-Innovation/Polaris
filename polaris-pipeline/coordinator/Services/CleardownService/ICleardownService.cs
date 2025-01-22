using Microsoft.DurableTask.Client;
using System;
using System.Threading.Tasks;

namespace coordinator.Services.ClearDownService
{
    public interface IClearDownService
    {
        Task DeleteCaseAsync(DurableTaskClient client, string caseUrn, int caseId, Guid correlationId);
    }
}