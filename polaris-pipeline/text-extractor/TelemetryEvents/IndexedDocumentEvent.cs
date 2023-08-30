using System;
using System.Collections.Generic;
using Common.Telemetry;

namespace text_extractor.TelemetryEvents
{
    public class IndexedDocumentEvent : BaseTelemetryEvent
    {
        private const string ocrDurationSeconds = nameof(ocrDurationSeconds);
        private const string indexDurationSeconds = nameof(indexDurationSeconds);
        private const string indexSettledDurationSeconds = nameof(indexSettledDurationSeconds);

        public Guid CorrelationId { get; set; }
        public long CaseId { get; set; }
        public string DocumentId { get; set; }
        public long VersionId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime OcrCompletedTime { get; set; }
        public DateTime IndexStoredTime { get; set; }
        public DateTime EndTime { get; set; }
        public int PageCount { get; set; }
        public int LineCount { get; set; }
        public int WordCount { get; set; }

        public IndexedDocumentEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        public override (IDictionary<string, string>, IDictionary<string, double>) ToTelemetryEventProps()
        {
            return (
                new Dictionary<string, string>
                {
                    { nameof(CorrelationId), CorrelationId.ToString() },
                    { nameof(CaseId), CaseId.ToString() },
                    { nameof(DocumentId), DocumentId.ToString() },
                    { nameof(VersionId), VersionId.ToString() },
                    { nameof(StartTime), StartTime.ToString("o") },
                    { nameof(EndTime), EndTime.ToString("o") },
                },
                new Dictionary<string, double>
                {
                    { durationSeconds, GetDurationSeconds(StartTime, EndTime) },
                    { nameof(PageCount), PageCount },
                    { nameof(LineCount), LineCount },
                    { nameof(WordCount), WordCount },
                    { nameof(ocrDurationSeconds), GetDurationSeconds(StartTime, OcrCompletedTime) },
                    { nameof(indexDurationSeconds), GetDurationSeconds(OcrCompletedTime, IndexStoredTime) },
                    { nameof(indexSettledDurationSeconds), GetDurationSeconds(IndexStoredTime, EndTime) }
                }
            );
        }
    }
}