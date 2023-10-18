using System;
using System.Collections.Generic;
using System.Linq;
using Common.Telemetry;
using pdf_generator.Services.DocumentRedactionService.RedactionProvider;

namespace pdf_generator.TelemetryEvents
{
  public class RedactedDocumentEvent : BaseTelemetryEvent
  {
    private const string redactionCount = nameof(redactionCount);
    protected const string sanitizedDurationSeconds = nameof(sanitizedDurationSeconds);

    public Guid CorrelationId;
    public string CaseId;
    public string DocumentId;
    public Dictionary<int, int> RedactionPageCounts;
    public long OriginalBytes;
    public long Bytes;
    public DateTime StartTime;
    public DateTime SanitizedTime;
    public DateTime EndTime;
    public ProviderType ProviderType;
    public string ProviderDetails;
    public ProviderReason ProviderReason;
    public int OriginalNullCharCount;
    public int NullCharCount;
    public int PageCount;
    public bool IsSanitizeBroken;

    public RedactedDocumentEvent(
        Guid correlationId,
        string caseId,
        string documentId,
        Dictionary<int, int> redactionPageCounts,
        ProviderType providerType,
        string providerDetails)
    {
      CorrelationId = correlationId;
      CaseId = caseId;
      DocumentId = documentId;
      RedactionPageCounts = redactionPageCounts;
      ProviderType = providerType;
      ProviderDetails = providerDetails;
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
                    { nameof(SanitizedTime), SanitizedTime.ToString("o") },
                    { nameof(EndTime), EndTime.ToString("o") },
                    { nameof(RedactionPageCounts), string.Join(",", RedactionPageCounts.Select(x => $"{x.Key}:{x.Value}")) },
                    { nameof(ProviderType), ProviderType.ToString() },
                    { nameof(ProviderDetails), ProviderDetails.ToString() },
                    { nameof(ProviderReason), ProviderReason.ToString() },
                    { nameof(IsSanitizeBroken), IsSanitizeBroken.ToString() },
          },
          new Dictionary<string, double?>
          {
                    { redactionCount, RedactionPageCounts.Select(x => x.Value).Sum() },
                    { durationSeconds, GetDurationSeconds(StartTime, EndTime) },
                    { sanitizedDurationSeconds, GetDurationSeconds(StartTime, SanitizedTime) },
                    { nameof(OriginalBytes), OriginalBytes },
                    { nameof(Bytes), Bytes },
                    { nameof(OriginalNullCharCount), OriginalNullCharCount },
                    { nameof(NullCharCount), NullCharCount },
                    { nameof(PageCount), PageCount }
          }
      );
    }
  }
}
