using System;
using System.Collections.Generic;
using Common.Telemetry;

namespace pdf_generator.TelemetryEvents
{
    public class ConvertedDocumentEvent : BaseTelemetryEvent
    {
        private readonly Guid _correlationId;
        private readonly string _caseId;
        private readonly string _documentId;
        private readonly string _versionId;
        private readonly string _fileType;
        private readonly long _originalBytes;
        private readonly long _bytes;
        private readonly DateTime _startTime;
        private readonly DateTime _endTime;

        public ConvertedDocumentEvent(
            Guid correlationId,
            string caseId,
            string documentId,
            string versionId,
            string fileType,
            long originalBytes,
            long bytes,
            DateTime startTime,
            DateTime endTime)
        {
            _correlationId = correlationId;
            _caseId = caseId;
            _documentId = documentId;
            _versionId = versionId;
            _fileType = fileType;
            _originalBytes = originalBytes;
            _bytes = bytes;
            _startTime = startTime;
            _endTime = endTime;
        }

        public override (IDictionary<string, string>, IDictionary<string, double>) ToTelemetryEventProps()
        {
            return (
                new Dictionary<string, string>
                {
                    { nameof(_correlationId), _correlationId.ToString() },
                    { nameof(_caseId), _documentId },
                    { nameof(_documentId), _documentId },
                    { nameof(_versionId), _versionId },
                    { nameof(_fileType), _fileType},
                    { nameof(_startTime), _startTime.ToString("o") },
                    { nameof(_endTime), _endTime.ToString("o") },
                },
                new Dictionary<string, double>
                {
                    { durationSeconds, GetDurationSeconds(_startTime, _endTime) },
                    { nameof(_originalBytes), _originalBytes },
                    { nameof(_bytes), _bytes }
                }
            );
        }
    }
}