namespace PolarisGateway.TelemetryEvents
{
    public class DocumentModifiedEvent : BaseRequestEvent
    {
        public DocumentModifiedEvent(
            int caseId,
            string documentId)
        {
            CaseId = caseId;
            DocumentId = documentId;
        }
    }
}