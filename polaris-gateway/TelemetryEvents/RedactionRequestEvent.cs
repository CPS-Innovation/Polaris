namespace PolarisGateway.TelemetryEvents
{
    public class RedactionRequestEvent : BaseRequestEvent
    {
        public RedactionRequestEvent(
            long caseId,
            string documentId)
        {
            CaseId = caseId;
            DocumentId = documentId;
        }
    }
}