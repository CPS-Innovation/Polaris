using System;
using System.Collections.Generic;
using System.Linq;
using Common.Telemetry;
using pdf_generator.Services.DocumentRedactionService.RedactionImplementation;

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
    public ImplementationType ImplementationType;
    public int OriginalNullCharCount;
    public int NullCharCount;

    public RedactedDocumentEvent(
        Guid correlationId,
        string caseId,
        string documentId,
        Dictionary<int, int> redactionPageCounts,
        ImplementationType implementationType)
    {
      CorrelationId = correlationId;
      CaseId = caseId;
      DocumentId = documentId;
      RedactionPageCounts = redactionPageCounts;
      ImplementationType = implementationType;
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
                    { nameof(RedactionPageCounts), string.Join(",", RedactionPageCounts.Select(x => $"{x.Key}:{x.Value}")) },
                    { nameof(ImplementationType), ImplementationType.ToString() }
          },
          new Dictionary<string, double?>
          {
                    { redactionCount, RedactionPageCounts.Select(x => x.Value).Sum() },
                    { durationSeconds, GetDurationSeconds(StartTime, EndTime) },
                    { nameof(OriginalBytes), OriginalBytes },
                    { nameof(Bytes), Bytes },
                    { nameof(OriginalNullCharCount), OriginalNullCharCount },
                    { nameof(NullCharCount), NullCharCount },
          }
      );
    }
  }
}
