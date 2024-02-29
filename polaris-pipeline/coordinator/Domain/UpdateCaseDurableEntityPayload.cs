using coordinator.Durable.Entity;

namespace coordinator.Domain
{
    internal class UpdateCaseDurableEntityPayload
    {
        public CaseOrchestrationPayload CaseOrchestrationPayload { get; set; }

        public CaseDurableEntity Tracker { get; set; }
    }
}
