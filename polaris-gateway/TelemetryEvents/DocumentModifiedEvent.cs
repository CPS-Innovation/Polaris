namespace PolarisGateway.TelemetryEvents
{
    public class DocumentModifiedEvent : BaseRequestEvent
    {
        public DocumentModifiedEvent(
            long caseId,
            string documentId)
        {
            CaseId = caseId;
            DocumentId = documentId;
        }
    }
}