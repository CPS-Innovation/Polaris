namespace coordinator.Domain
{
    internal class UpdateTrackerPayload
    {
        public CaseOrchestrationPayload CaseOrchestrationPayload { get; set; }

        public Functions.DurableEntityFunctions.Tracker Tracker { get; set; }
    }
}
