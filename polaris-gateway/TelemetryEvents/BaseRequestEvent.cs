using Common.Telemetry;
using System;
using System.Collections.Generic;

namespace PolarisGateway.TelemetryEvents
{
    public class BaseRequestEvent : BaseTelemetryEvent
    {
        private const string RequestJsonLength = nameof(RequestJsonLength);
        public Guid CorrelationId;
        public long CaseId;
        public string DocumentId;
        public string RequestJson;
        public bool IsRequestValid;
        public bool IsRequestJsonValid;
        public bool IsSuccess;

        public override (IDictionary<string, string>, IDictionary<string, double?>) ToTelemetryEventProps()
        {
            return (
            new Dictionary<string, string>
            {
                    { nameof(CorrelationId), CorrelationId.ToString() },
                    { nameof(CaseId), CaseId.ToString() },
                    { nameof(DocumentId), DocumentId.ToString() },

                    { nameof(IsRequestValid), IsRequestValid.ToString() },
                    { nameof(IsRequestJsonValid), IsRequestJsonValid.ToString() },
                    { nameof(IsSuccess), IsSuccess.ToString() },
                    { nameof(RequestJson), RequestJson },
            },
            new Dictionary<string, double?>
            {
                    { RequestJsonLength, RequestJson?.Length ?? 0 },
            }
        );
        }
    }
}