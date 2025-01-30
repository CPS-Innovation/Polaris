using System;
using System.Collections.Generic;
using System.Linq;
using Common.Telemetry;

namespace text_extractor.coordinator
{
    public class IndexedDocumentEvent : BaseTelemetryEvent
    {
        private const string ocrDurationSeconds = nameof(ocrDurationSeconds);
        private const string indexDurationSeconds = nameof(indexDurationSeconds);
        private const string indexSettledDurationSeconds = nameof(indexSettledDurationSeconds);

        public Guid CorrelationId { get; set; }
        public string CaseUrn { get; set; }
        public int CaseId { get; set; }
        public string DocumentId { get; set; }
        // public int? DocumentTypeId { get; set; }
        // public string DocumentType { get; set; }
        // public string DocumentCategory { get; set; }
        public long VersionId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime OcrCompletedTime { get; set; }
        public DateTime IndexStoredTime { get; set; }
        public DateTime EndTime { get; set; }
        public long PageCount { get; set; }
        public long LineCount { get; set; }
        public long WordCount { get; set; }
        public long IndexSettleTargetCount { get; set; }
        public bool DidIndexSettle { get; set; }
        public List<long> WaitRecordCounts { get; set; }
        public bool DidOrchestratorReplay { get; set; }

        public IndexedDocumentEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        public override (IDictionary<string, string>, IDictionary<string, double?>) ToTelemetryEventProps()
        {
            return (
                new Dictionary<string, string>
                {
                    { nameof(CorrelationId), CorrelationId.ToString() },
                    { nameof(CaseUrn), CaseUrn },
                    { nameof(CaseId), CaseId.ToString() },
                    { nameof(DocumentId), DocumentId.ToString() },
                    // { nameof(DocumentTypeId), DocumentTypeId?.ToString() },
                    // { nameof(DocumentType), DocumentType?.ToString()},
                    // { nameof(DocumentCategory), DocumentCategory?.ToString()},
                    { nameof(VersionId), VersionId.ToString() },
                    { nameof(StartTime), StartTime.ToString("o") },
                    { nameof(EndTime), EndTime.ToString("o") },
                    { nameof(DidIndexSettle), DidIndexSettle.ToString() },
                    { nameof(WaitRecordCounts), string.Join(",", WaitRecordCounts ?? Enumerable.Empty<long>()) },
                },
                new Dictionary<string, double?>
                {
                    { durationSeconds, GetDurationSeconds(StartTime, EndTime) },
                    { nameof(PageCount), PageCount },
                    { nameof(LineCount), LineCount },
                    { nameof(WordCount), WordCount },
                    { nameof(IndexSettleTargetCount), IndexSettleTargetCount},
                    { nameof(ocrDurationSeconds), GetDurationSeconds(StartTime, OcrCompletedTime) },
                    { nameof(indexDurationSeconds), GetDurationSeconds(OcrCompletedTime, IndexStoredTime) },
                    { nameof(indexSettledDurationSeconds), GetDurationSeconds(IndexStoredTime, EndTime) }
                }
            );
        }
    }
}