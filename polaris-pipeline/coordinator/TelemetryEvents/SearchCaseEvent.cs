using System;
using System.Collections.Generic;
using Common.Telemetry;

namespace coordinator.TelemetryEvents
{
    public class SearchCaseEvent : BaseTelemetryEvent
    {
        private const string DocumentIdsLength = nameof(DocumentIdsLength);
        public Guid CorrelationId;
        public long CaseId;
        public string DocumentIds;
        public SearchCaseEvent(
            Guid correlationId,
            long caseId,
            string documentIds)
        {
            CorrelationId = correlationId;
            CaseId = caseId;
            DocumentIds = documentIds;
        }

        public override (IDictionary<string, string>, IDictionary<string, double?>) ToTelemetryEventProps()
        {
            return (
                new Dictionary<string, string>
                {
                    { nameof(CorrelationId), CorrelationId.ToString() },
                    { nameof(CaseId), CaseId.ToString() },
                    { nameof(DocumentIds), DocumentIds },
                },
                new Dictionary<string, double?>
                {
                    { DocumentIdsLength, DocumentIds?.Length ?? 0 },
                }
            );
        }
    }
}