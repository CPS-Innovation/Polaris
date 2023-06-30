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

        private readonly Guid _correlationId;
        private readonly long _caseId;
        private readonly string _documentId;
        private readonly long _versionId;
        private readonly DateTime _startTime;
        private readonly DateTime _ocrCompletedTime;
        private readonly DateTime _indexStoredTime;
        private readonly DateTime _endTime;
        private readonly int _pageCount;
        private readonly int _lineCount;
        private readonly int _wordCount;

        public IndexedDocumentEvent(
            Guid correlationId,
            long caseId,
            string documentId,
            long versionId,
            int pageCount,
            int lineCount,
            int wordCount,
            DateTime startTime,
            DateTime ocrCompletedTime,
            DateTime indexStoredTime,
            DateTime endTime)
        {
            _correlationId = correlationId;
            _caseId = caseId;
            _documentId = documentId;
            _versionId = versionId;
            _pageCount = pageCount;
            _lineCount = lineCount;
            _wordCount = wordCount;
            _startTime = startTime;
            _ocrCompletedTime = ocrCompletedTime;
            _indexStoredTime = indexStoredTime;
            _endTime = endTime;
        }

        public override (IDictionary<string, string>, IDictionary<string, double>) ToTelemetryEventProps()
        {
            return (
                new Dictionary<string, string>
                {
                    { nameof(_correlationId), _correlationId.ToString() },
                    { nameof(_caseId), _caseId.ToString() },
                    { nameof(_documentId), _documentId.ToString() },
                    { nameof(_versionId), _versionId.ToString() },
                    { nameof(_startTime), _startTime.ToString("o") },
                    { nameof(_endTime), _endTime.ToString("o") },
                },
                new Dictionary<string, double>
                {
                    { durationSeconds, GetDurationSeconds(_startTime, _endTime) },
                    { nameof(_pageCount), _pageCount },
                    { nameof(_lineCount), _lineCount },
                    { nameof(_wordCount), _wordCount },
                    { nameof(ocrDurationSeconds), GetDurationSeconds(_startTime, _ocrCompletedTime) },
                    { nameof(indexDurationSeconds), GetDurationSeconds(_ocrCompletedTime, _indexStoredTime) },
                    { nameof(indexSettledDurationSeconds), GetDurationSeconds(_indexStoredTime, _endTime) }
                }
            );
        }
    }
}