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

        public override (IDictionary<string, string>, IDictionary<string, double?>) ToTelemetryEventProps()
        {
            return (
                new Dictionary<string, string>
                {
                    { nameof(CorrelationId), CorrelationId.ToString() },
                    { nameof(CaseId), CaseId.ToString() },
                    { nameof(ReclassificationType), ReclassificationType },
                    { nameof(DocumentRenamed), DocumentRenamed.ToString() }
                },
                new Dictionary<string, double?>
                {
                    { nameof(DocumentId), int.Parse(DocumentId) },
                    { nameof(ResponseDocumentId), ResponseDocumentId },
                    { nameof(OriginalDocumentTypeId), OriginalDocumentTypeId },
                    { nameof(NewDocumentTypeId), NewDocumentTypeId }
                }
            );
        }
    }
}