using System;
using System.Collections.Generic;
using System.Linq;
using Common.Telemetry;

namespace pdf_generator.TelemetryEvents
{
  public class RedactedDocumentEvent : BaseTelemetryEvent
  {
    private const string redactionCount = nameof(redactionCount);

    public Guid CorrelationId;
    public string CaseId;
    public string DocumentId;
    public Dictionary<int, int> RedactionPageCounts;
    public long OriginalBytes;
    public long Bytes;
    public DateTime StartTime;
    public DateTime EndTime;

    public RedactedDocumentEvent(
        Guid correlationId,
        string caseId,
        string documentId,
        Dictionary<int, int> redactionPageCounts)
    {
      CorrelationId = correlationId;
      CaseId = caseId;
      DocumentId = documentId;
      RedactionPageCounts = redactionPageCounts;
    }

    public override (IDictionary<string, string>, IDictionary<string, double?>) ToTelemetryEventProps()
    {
      return (
          new Dictionary<string, string>
          {
                    { nameof(CorrelationId), CorrelationId.ToString() },
                    { nameof(CaseId), CaseId },
                    { nameof(DocumentId), EnsureNumericId(DocumentId) },
                    { nameof(StartTime), StartTime.ToString("o") },
                    { nameof(EndTime), EndTime.ToString("o") },
                    { nameof(RedactionPageCounts), string.Join(",", RedactionPageCounts.Select(x => $"{x.Key}:{x.Value}")) }
          },
          new Dictionary<string, double?>
          {
                    { redactionCount, RedactionPageCounts.Select(x => x.Value).Sum()},
                    { durationSeconds, GetDurationSeconds(StartTime, EndTime) },
                    { nameof(OriginalBytes), OriginalBytes },
                    { nameof(Bytes), Bytes }
          }
      );
    }
  }
}
