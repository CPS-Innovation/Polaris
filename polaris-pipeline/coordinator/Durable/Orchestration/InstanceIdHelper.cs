namespace coordinator.Durable.Orchestration;

public static class InstanceIdHelper
{
    public static string OrchestratorKey(string caseId) => $"[{caseId}]";
    //public static string EntityKey(string caseId) => $"@{nameof(CaseDurableEntity).ToLower()}@{OrchestratorKey(caseId)}";

}