using coordinator.Durable.Payloads.Domain;

namespace coordinator.Domain;

public class DocumentDelta
{
    public CmsDocumentEntity Document { get; set; }

    public DocumentDeltaType DeltaType { get; set; }
}
