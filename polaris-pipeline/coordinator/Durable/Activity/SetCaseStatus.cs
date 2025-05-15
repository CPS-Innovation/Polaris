using Common.Dto.Response.Documents;
using coordinator.Domain;
using coordinator.Services;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;

namespace coordinator.Durable.Activity;

public class SetCaseStatus(IStateStorageService stateStorageService)
{
    [Function(nameof(SetCaseStatus))]
    public async Task<bool> Run([ActivityTrigger] SetCaseStatusPayload payload)
    {
        var state = await stateStorageService.GetStateAsync(payload.CaseId);
        state.Status = payload.Status;

        switch (state.Status)
        {
            case CaseRefreshStatus.Running:
                state.Running = payload.UpdatedAt;
                break;

            case CaseRefreshStatus.DocumentsRetrieved:
                if (state.Running != null)
                {
                    state.Retrieved = (float)((payload.UpdatedAt - state.Running).Value.TotalMilliseconds / 1000.0);
                }

                break;

            case CaseRefreshStatus.Completed:
                if (state.Running != null)
                {
                    state.Completed = (float)((payload.UpdatedAt - state.Running).Value.TotalMilliseconds / 1000.0);
                }

                break;

            case CaseRefreshStatus.Failed:
                if (state.Running != null)
                {
                    state.Failed = (float)((payload.UpdatedAt - state.Running).Value.TotalMilliseconds / 1000.0);
                    state.FailedReason = payload.FailedReason;
                }
                break;
        }

        return await stateStorageService.UpdateStateAsync(state);
    }
}
