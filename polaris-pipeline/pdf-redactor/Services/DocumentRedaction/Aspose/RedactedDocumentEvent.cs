using Common.Telemetry;
using pdf_redactor.Services.DocumentRedaction.Aspose;

namespace pdf_redactor.TelemetryEvents
{
  public class RedactedDocumentEvent : BaseTelemetryEvent
  {
    private const string redactionCount = nameof(redactionCount);
    protected const string sanitizedDurationSeconds = nameof(sanitizedDurationSeconds);
    protected const string annotationsDurationSeconds = nameof(annotationsDurationSeconds);
    protected const string finaliseAnnotationsDurationSeconds = nameof(finaliseAnnotationsDurationSeconds);
    public Guid CorrelationId;
    public int CaseId;
    public string DocumentId;
    public Dictionary<int, int> RedactionPageCounts;
    public long OriginalBytes;
    public long Bytes;
    public DateTime StartTime;
    public DateTime EndTime;
    public ProviderType ProviderType;
    public string ProviderDetails;
    public int OriginalNullCharCount;
    public int NullCharCount;
    public int PageCount;
    public string PdfFormat;
    public DateTime AddAnnotationsStartTime;
    public DateTime AddAnnotationsEndTime;
    public DateTime FinaliseAnnotationsStartTime;
    public DateTime FinaliseAnnotationsEndTime;
    public DateTime SanitiseStartTime;
    public DateTime SanitiseEndTime;

    public RedactedDocumentEvent(
        Guid correlationId,
        int caseId,
        string documentId,
        Dictionary<int, int> redactionPageCounts,
        ProviderType providerType,
        string providerDetails,
        DateTime startTime,
        long originalBytes)
    {
      CorrelationId = correlationId;
      CaseId = caseId;
      DocumentId = documentId;
      RedactionPageCounts = redactionPageCounts;
      ProviderType = providerType;
      ProviderDetails = providerDetails;
      StartTime = startTime;
      OriginalBytes = originalBytes;
    }

    public override (IDictionary<string, string>, IDictionary<string, double?>) ToTelemetryEventProps()
    {
      return (
          new Dictionary<string, string>
          {
                    { nameof(CorrelationId), CorrelationId.ToString() },
                    { nameof(CaseId), CaseId.ToString() },
                    { nameof(DocumentId), EnsureNumericId(DocumentId) },
                    { nameof(StartTime), StartTime.ToString("o") },
                    { nameof(EndTime), EndTime.ToString("o") },
                    { nameof(RedactionPageCounts), string.Join(",", RedactionPageCounts?.Select(x => $"{x.Key}:{x.Value}")) },
                    { nameof(ProviderType), ProviderType.ToString() },
                    { nameof(ProviderDetails), ProviderDetails?.ToString() },
                    { nameof(PdfFormat), PdfFormat?.ToString() },
          },
          new Dictionary<string, double?>
          {
                    { redactionCount, RedactionPageCounts?.Select(x => x.Value).Sum() },
                    { durationSeconds, GetDurationSeconds(StartTime, EndTime) },
                    { nameof(OriginalBytes), OriginalBytes },
                    { nameof(Bytes), Bytes },
                    { nameof(OriginalNullCharCount), OriginalNullCharCount },
                    { nameof(NullCharCount), NullCharCount },
                    { nameof(PageCount), PageCount },
                    { annotationsDurationSeconds, GetDurationSeconds(AddAnnotationsStartTime, AddAnnotationsEndTime)},
                    { finaliseAnnotationsDurationSeconds, GetDurationSeconds(FinaliseAnnotationsStartTime, FinaliseAnnotationsEndTime)},
                    { sanitizedDurationSeconds, GetDurationSeconds(SanitiseStartTime, SanitiseEndTime)}
          }
      );
    }
  }
}
