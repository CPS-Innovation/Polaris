namespace coordinator.Durable.Payloads.Domain;

public enum DocumentDeltaType
{
    DoesNotRequireRefresh = 0,
    RequiresPdfRefresh = 1,
    RequiresIndexing = 2,
}