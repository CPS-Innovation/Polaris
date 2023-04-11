using coordinator.Functions.DurableEntity.Entity;

namespace coordinator.Domain
{
    internal class UpdateTrackerPayload
    {
        public CaseOrchestrationPayload CaseOrchestrationPayload { get; set; }

        public TrackerEntity Tracker { get; set; }
    }
}
