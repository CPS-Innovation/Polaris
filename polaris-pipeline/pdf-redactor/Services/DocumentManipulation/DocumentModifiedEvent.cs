using Common.Telemetry;
using pdf_redactor.Services.DocumentRedaction.Aspose;

namespace pdf_redactor.Services.DocumentManipulation
{
    public class DocumentModifiedEvent(
        Guid correlationId,
        int caseId,
        string documentId,
        int[] pageNumbersRemoved,
        int[] pageNumbersRotated,
        DateTime startTime,
        long originalBytes) : BaseTelemetryEvent
    {
        public Guid CorrelationId = correlationId;
        public int CaseId = caseId;
        public string DocumentId = documentId;
        public int[] PageNumbersRemoved = pageNumbersRemoved;
        public int[] PageNumbersRotated = pageNumbersRotated;
        public long OriginalBytes = originalBytes;
        public long Bytes;
        public DateTime StartTime = startTime;
        public DateTime EndTime;
        public ProviderType ProviderType;
        public string? ProviderDetails;
        public int PageCount;
        public int PagesRemovedCount => PageNumbersRemoved.Length;
        public int PagesRotatedCount => PageNumbersRotated.Length;
        public string? PdfFormat;

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
                    { nameof(PageNumbersRemoved), string.Join(",", PageNumbersRemoved) },
                    { nameof(PageNumbersRotated), string.Join(",", PageNumbersRotated) },
                    { nameof(ProviderType), ProviderType.ToString() },
                    { nameof(ProviderDetails), ProviderDetails?.ToString() ?? string.Empty },
                    { nameof(PdfFormat), PdfFormat?.ToString() ?? string.Empty },
                },
                new Dictionary<string, double?>
                {
                    { nameof(PagesRemovedCount), PagesRemovedCount },
                    { nameof(PagesRotatedCount), PagesRotatedCount },
                    { durationSeconds, GetDurationSeconds(StartTime, EndTime) },
                    { nameof(OriginalBytes), OriginalBytes },
                    { nameof(Bytes), Bytes },
                    { nameof(PageCount), PageCount }
                }
            );
        }
    }
}