namespace PolarisGateway.TelemetryEvents
{
    public class RenameDocumentRequestEvent : BaseRequestEvent
    {
        public RenameDocumentRequestEvent(
            long caseId,
            string documentId
        )
        {
            CaseId = caseId;
            DocumentId = documentId;
        }
    }
}