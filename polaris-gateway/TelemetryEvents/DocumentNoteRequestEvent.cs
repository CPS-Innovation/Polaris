namespace PolarisGateway.TelemetryEvents
{
    public class DocumentNoteRequestEvent : BaseRequestEvent
    {
        public DocumentNoteRequestEvent(
            long caseId,
            string documentId)
        {
            CaseId = caseId;
            DocumentId = documentId;
        }
    }
}