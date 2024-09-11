namespace PolarisGateway.TelemetryEvents
{
    public class DocumentReclassifiedEvent : BaseRequestEvent
    {
        public DocumentReclassifiedEvent(
            long caseId,
            string documentId)
        {
            CaseId = caseId;
            DocumentId = documentId;
        }
    }
}