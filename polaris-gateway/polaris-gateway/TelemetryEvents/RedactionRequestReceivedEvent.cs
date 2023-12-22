using System;
using System.Collections.Generic;
using Common.Telemetry;

namespace PolarisGateway.TelemetryEvents
{
    public class RedactionRequestReceivedEvent : BaseTelemetryEvent
    {
        private const string RequestJsonLength = nameof(RequestJsonLength);
        public Guid CorrelationId;
        public long CaseId;
        public string DocumentId;
        public string RequestJson;
        public bool IsRequestJsonValid;

        public RedactionRequestReceivedEvent(
            long caseId,
            string documentId)
        {
            CaseId = caseId;
            DocumentId = documentId;
        }

        public override (IDictionary<string, string>, IDictionary<string, double?>) ToTelemetryEventProps()
        {
            return (
                new Dictionary<string, string>
                {
                    { nameof(CorrelationId), CorrelationId.ToString() },
                    { nameof(CaseId), CaseId.ToString() },

                    { nameof(DocumentId), DocumentId.ToString() },
                    { nameof(RequestJson), RequestJson },
                    { nameof(IsRequestJsonValid), IsRequestJsonValid.ToString() },
                },
                new Dictionary<string, double?>
                {
                    { RequestJsonLength, RequestJson?.Length ?? 0 },
                }
            );
        }
    }
}