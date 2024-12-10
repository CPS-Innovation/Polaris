using coordinator.Durable.Payloads;

namespace coordinator.Domain;

public class CompleteIndexPayload
{
    public long TargetCount { get; set; }

    public DocumentPayload Payload { get; set; }
}
