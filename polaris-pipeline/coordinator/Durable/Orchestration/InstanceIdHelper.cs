namespace coordinator.Durable.Orchestration;

public static class InstanceIdHelper
{
    public static string OrchestratorKey(string caseId) => $"[{caseId}]";
    public static string DocumentOrchestratorKey(string caseId, string polarisDocumentId) => $"{OrchestratorKey(caseId)}-{polarisDocumentId}";
}