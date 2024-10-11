namespace PolarisGateway.TelemetryEvents
{
    public class RenameDocumentRequestEvent : BaseRequestEvent
    {
        public RenameDocumentRequestEvent(
            int caseId,
            string documentId
        )
        {
            CaseId = caseId;
            DocumentId = documentId;
        }
    }
}