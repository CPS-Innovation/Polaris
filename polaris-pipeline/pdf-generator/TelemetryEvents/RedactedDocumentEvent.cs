using System;
using System.Collections.Generic;
using System.Linq;
using Common.Telemetry;

namespace pdf_generator.TelemetryEvents
{
  public class RedactedDocumentEvent : BaseTelemetryEvent
  {
    private const string redactionCount = nameof(redactionCount);

    private readonly Guid _correlationId;
    private readonly string _caseId;
    private readonly string _documentId;
    private readonly Dictionary<int, int> _redactionPageCounts;
    private readonly long _originalBytes;
    private readonly long _bytes;
    private readonly DateTime _startTime;
    private readonly DateTime _endTime;

    public RedactedDocumentEvent(
        Guid correlationId,
        string caseId,
        string documentId,
        Dictionary<int, int> redactionPageCounts,
        long originalBytes,
        long bytes,
        DateTime startTime,
        DateTime endTime)
    {
      _correlationId = correlationId;
      _caseId = caseId;
      _documentId = documentId;
      _redactionPageCounts = redactionPageCounts;
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
                    { nameof(_caseId), _caseId },
                    { nameof(_documentId), EnsureNumericId(_documentId) },
                    { nameof(_startTime), _startTime.ToString("o") },
                    { nameof(_endTime), _endTime.ToString("o") },
                    {nameof(_redactionPageCounts), string.Join(",", _redactionPageCounts.Select(x => $"{x.Key}:{x.Value}"))}
          },
          new Dictionary<string, double>
          {
                    { redactionCount, _redactionPageCounts.Select(x => x.Value).Sum()},
                    { durationSeconds, GetDurationSeconds(_startTime, _endTime) },
                    { nameof(_originalBytes), _originalBytes },
                    { nameof(_bytes), _bytes }
          }
      );
    }
  }
}