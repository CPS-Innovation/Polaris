using coordinator.Domain;
using coordinator.Durable.Payloads.Domain;
using System.Threading.Tasks;

namespace coordinator.Services;

public interface IStateStorageService
{
    Task<CaseDurableEntityState> GetStateAsync(int caseId);

    Task<bool> UpdateStateAsync(CaseDurableEntityState caseState);

    Task<CaseDurableEntityDocumentsState> GetDurableEntityDocumentsStateAsync(int caseId);

    Task<bool> UpdateDurableEntityDocumentsStateAsync(int caseId, CaseDurableEntityDocumentsState caseDurableEntityDocumentsState);

    Task<CaseDeltasEntity> GetCaseDeltasEntityAsync(int caseId);

    Task<bool> UpdateCaseDeltasEntityAsync(int caseId, CaseDeltasEntity caseDeltasEntity);

    Task<GetCaseDocumentsResponse> GetCaseDocumentsAsync(int caseId);

    Task<bool> UpdateCaseDocumentsAsync(int caseId, GetCaseDocumentsResponse caseDocumentsResponse);
}
