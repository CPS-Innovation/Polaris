namespace PolarisGateway.TelemetryEvents
{
    public class DocumentReclassifiedEvent : BaseRequestEvent
    {
        public DocumentReclassifiedEvent(
            int caseId,
            string documentId)
        {
            CaseId = caseId;
            DocumentId = documentId;
        }
    }
}