using coordinator.Durable.Entity;
using coordinator.Durable.Payloads;

namespace coordinator.Domain
{
    internal class UpdateCaseDurableEntityPayload
    {
        public CaseOrchestrationPayload CaseOrchestrationPayload { get; set; }

        public CaseDurableEntity Tracker { get; set; }
    }
}
