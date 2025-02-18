using Common.Dto.Response.Documents;
using coordinator.Domain;
using coordinator.Durable.Payloads.Domain;
using coordinator.Services;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;

namespace coordinator.Durable.Activity;

public class Reset(IStateStorageService stateStorageService)
{
    [Function(nameof(Reset))]
    public async Task<bool> RunAsync([ActivityTrigger] int caseId)
    {
        var state = new CaseDurableEntityState
        {
            CaseId = caseId,
            Status = CaseRefreshStatus.NotStarted,
            Running = null,
            Retrieved = null,
            Completed = null,
            Failed = null,
            FailedReason = null
        };

        return await stateStorageService.UpdateStateAsync(state);
    }
}
