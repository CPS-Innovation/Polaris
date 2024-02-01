using System;
using System.Collections.Generic;
using Common.Telemetry;

namespace pdf_generator.TelemetryEvents
{
    public class ConvertedDocumentEvent : BaseTelemetryEvent
    {
        public Guid CorrelationId { get; set; }
        public string CaseUrn { get; set; }
        public string CaseId { get; set; }
        public string DocumentId { get; set; }
        public string VersionId { get; set; }
        public string FileType { get; set; }
        public long OriginalBytes { get; set; }
        public long Bytes { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public ConvertedDocumentEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        public override (IDictionary<string, string>, IDictionary<string, double?>) ToTelemetryEventProps()
        {
            return (
                new Dictionary<string, string>
                {
                    { nameof(CorrelationId), CorrelationId.ToString() },
                    { nameof(CaseUrn), CaseUrn},
                    { nameof(CaseId), CaseId },
                    { nameof(DocumentId), EnsureNumericId(DocumentId) },
                    { nameof(VersionId), VersionId },
                    { nameof(FileType), FileType},
                    { nameof(StartTime), StartTime.ToString("o") },
                    { nameof(EndTime), EndTime.ToString("o") },
                },
                new Dictionary<string, double?>
                {
                    { durationSeconds, GetDurationSeconds(StartTime, EndTime) },
                    { nameof(OriginalBytes), OriginalBytes },
                    { nameof(Bytes), Bytes }
                }
            );
        }
    }
}