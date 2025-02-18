using System.Collections.Generic;

namespace coordinator.Domain;

public class SetDocumentRefreshStatusesPayload
{
    public SetDocumentRefreshStatusesPayload()
    {
    }

    public int CaseId { get; set; }

    public List<RefreshDocumentOrchestratorResponse> DocumentRefreshStatuses { get; set; } = [];
}
