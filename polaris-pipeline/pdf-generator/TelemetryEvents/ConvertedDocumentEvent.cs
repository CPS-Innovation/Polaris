using System;
using System.Collections.Generic;
using Common.Telemetry;

namespace pdf_generator.TelemetryEvents
{
    public class ConvertedDocumentEvent : BaseTelemetryEvent
    {
        public Guid CorrelationId { get; set; }
        public string CaseUrn { get; set; } = string.Empty;
        public string CaseId { get; set; } = string.Empty;
        public string DocumentId { get; set; } = string.Empty;
        public string VersionId { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long OriginalBytes { get; set; } = 0;
        public long Bytes { get; set; } = 0;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string ConversionHandler { get; set; } = string.Empty;
        public string FailureReason { get; set; } = string.Empty;

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
                    { nameof(FileType), FileType },
                    { nameof(StartTime), StartTime.ToString("o") },
                    { nameof(EndTime), EndTime.ToString("o") },
                    { nameof(ConversionHandler), ConversionHandler },
                    { nameof(FailureReason), FailureReason }
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