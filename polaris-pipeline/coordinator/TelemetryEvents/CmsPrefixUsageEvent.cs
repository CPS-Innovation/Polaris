using System;
using System.Collections.Generic;
using Common.Telemetry;

namespace coordinator.TelemetryEvents
{
    /// <summary>
    /// Raised when a non-legacy endpoint receives a materialId using the temporarily-supported
    /// "CMS-" prefix, rather than a bare numeric id. Used to gauge real-world usage before the
    /// "CMS-" prefix support is removed from non-legacy endpoints (Stage 3 deprecation).
    /// </summary>
    public class CmsPrefixUsageEvent : BaseTelemetryEvent
    {
        public Guid CorrelationId;
        public long CaseId;
        public string DocumentId;

        public CmsPrefixUsageEvent(
            Guid correlationId,
            int caseId,
            string documentId,
            string operationName)
        {
            CorrelationId = correlationId;
            CaseId = caseId;
            DocumentId = documentId;
            OperationName = operationName;
        }

        public override (IDictionary<string, string>, IDictionary<string, double?>) ToTelemetryEventProps()
        {
            return (
                new Dictionary<string, string>
                {
                    { nameof(CorrelationId), CorrelationId.ToString() },
                    { nameof(CaseId), CaseId.ToString() },
                    { nameof(DocumentId), DocumentId },
                },
                new Dictionary<string, double?>()
            );
        }
    }
}
