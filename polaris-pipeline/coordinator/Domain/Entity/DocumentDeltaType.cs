namespace coordinator.Domain.Entity;

public enum DocumentDeltaType
{
    DoesNotRequireRefresh = 0,
    RequiresPdfRefresh = 1,
    RequiresIndexing = 2,
}