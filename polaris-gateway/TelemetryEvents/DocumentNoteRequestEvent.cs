namespace PolarisGateway.TelemetryEvents
{
    public class DocumentNoteRequestEvent : BaseRequestEvent
    {
        public DocumentNoteRequestEvent(
            int caseId,
            string documentId)
        {
            CaseId = caseId;
            DocumentId = documentId;
        }
    }
}