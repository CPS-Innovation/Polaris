using coordinator.Functions.DurableEntity.Entity;

namespace coordinator.Domain
{
    internal class UpdateTrackerPayload
    {
        public CaseOrchestrationPayload CaseOrchestrationPayload { get; set; }

        public CaseDurableEntity Tracker { get; set; }
    }
}
