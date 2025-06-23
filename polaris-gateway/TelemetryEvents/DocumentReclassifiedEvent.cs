using System.Collections.Generic;

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

        public int ResponseDocumentId { get; set; }
        public string ReclassificationType { get; set; }
        public int? OriginalDocumentTypeId { get; set; }
        public int NewDocumentTypeId { get; set; }
        public bool DocumentRenamed { get; set; }
        public string DocumentRenameOperationName { get; set; }
    }
}