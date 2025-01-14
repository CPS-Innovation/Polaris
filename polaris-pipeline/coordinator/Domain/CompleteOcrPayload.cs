using coordinator.Durable.Payloads;
using System;

namespace coordinator.Domain;

public class CompleteOcrPayload
{
    public Guid OcrOperationId { get; set; }

    public DocumentPayload Payload { get; set; }
}