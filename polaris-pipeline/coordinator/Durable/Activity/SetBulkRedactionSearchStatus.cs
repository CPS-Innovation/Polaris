using Common.Dto.Response;
using coordinator.Durable.Payloads;
using coordinator.Services;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;

namespace coordinator.Durable.Activity;

public class SetBulkRedactionSearchStatus(IStateStorageService stateStorageService)
{
    [Function(nameof(SetBulkRedactionSearchStatus))]
    public async Task<bool> Run([ActivityTrigger] BulkRedactionSearchStatusPayload payload)
    {
        var state = await stateStorageService.GetBulkRedactionSearchStateAsync(payload.CaseId, payload.DocumentId, payload.VersionId, payload.SearchText);
        state.Status = payload.Status;

        switch (state.Status)
        {
            case BulkRedactionSearchStatus.GeneratingOcrDocument:
                state.SearchTerm = payload.SearchText;
                break;

            case BulkRedactionSearchStatus.SearchingDocument:
                state.OcrDocumentGeneratedAt = payload.OcrDocumentGeneratedAt;
                break;

            case BulkRedactionSearchStatus.Completed:
                state.DocumentSearchCompletedAt = payload.DocumentSearchCompletedAt;
                state.RedactionDefinitions = payload.RedactionDefinitions;
                state.CompletedAt = payload.CompletedAt;
                break;

            case BulkRedactionSearchStatus.Failed:
                state.FailureReason = payload.FailureReason;
                state.FailedAt = payload.FailedAt;
                break;
        }
        state.UpdatedAt = payload.UpdatedAt;

        return await stateStorageService.UpdateBulkRedactionSearchStateAsync(state);
    }
}
