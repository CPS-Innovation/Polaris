namespace PolarisGateway.TelemetryEvents
{
    public class RemoveDocumentPageEvent : BaseRequestEvent
    {
        public RemoveDocumentPageEvent(
            long caseId,
            string documentId)
        {
            CaseId = caseId;
            DocumentId = documentId;
        }
    }
}