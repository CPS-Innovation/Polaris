using System;
using System.Collections.Generic;
using Common.Telemetry;

namespace coordinator.TelemetryEvents
{
    public class RefreshedCaseEvent : BaseTelemetryEvent
    {
        public Guid CorrelationId;
        public long CaseId;
        public int? VersionId;
        public DateTime StartTime;
        public DateTime EndTime;
        public int CmsDocsCount;
        public int CmsDocsProcessedCount;
        public int PcdRequestsProcessedCount;

        public RefreshedCaseEvent(
            Guid correlationId,
            long caseId,
            int? versionId,
            DateTime startTime)
        {
            CorrelationId = correlationId;
            CaseId = caseId;
            VersionId = versionId;
            StartTime = startTime;
        }

        public override (IDictionary<string, string>, IDictionary<string, double?>) ToTelemetryEventProps()
        {
            return (
                new Dictionary<string, string>
                {
                    { nameof(CorrelationId), CorrelationId.ToString() },
                    { nameof(CaseId), CaseId.ToString() },
                    { nameof(VersionId), VersionId.ToString() },
                    { nameof(StartTime), StartTime.ToString("o") },
                    { nameof(EndTime), EndTime.ToString("o") },
                },
                new Dictionary<string, double?>
                {
                    { durationSeconds, GetDurationSeconds( StartTime,EndTime) },
                    { nameof(CmsDocsCount), CmsDocsCount },
                    { nameof(CmsDocsProcessedCount), CmsDocsProcessedCount },
                    { nameof(PcdRequestsProcessedCount), PcdRequestsProcessedCount },
                }
            );
        }
    }
}