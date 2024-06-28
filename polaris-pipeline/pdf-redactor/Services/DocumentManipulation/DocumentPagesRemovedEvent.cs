using Common.Telemetry;
using pdf_redactor.Services.DocumentRedaction.Aspose;

namespace pdf_redactor.Services.DocumentManipulation
{
    public class DocumentPagesRemovedEvent : BaseTelemetryEvent
    {
        public DocumentPagesRemovedEvent(
            Guid correlationId,
            string caseId,
            string documentId,
            int[] pageNumbersRemoved,
            DateTime startTime,
            long originalBytes)
        {
            CorrelationId = correlationId;
            CaseId = caseId;
            DocumentId = documentId;
            PageNumbersRemoved = pageNumbersRemoved;
            StartTime = startTime;
            OriginalBytes = originalBytes;
        }

        public Guid CorrelationId;
        public string CaseId;
        public string DocumentId;
        public int[] PageNumbersRemoved;
        public long OriginalBytes;
        public long Bytes;
        public DateTime StartTime;
        public DateTime EndTime;
        public ProviderType ProviderType;
        public string ProviderDetails;
        public int PageCount;
        public int PagesRemovedCount => PageNumbersRemoved.Length;
        public string PdfFormat;

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
                    { nameof(PageNumbersRemoved), string.Join(",", PageNumbersRemoved) },
                    { nameof(ProviderType), ProviderType.ToString() },
                    { nameof(ProviderDetails), ProviderDetails?.ToString() },
                    { nameof(PdfFormat), PdfFormat?.ToString() },
                },
                new Dictionary<string, double?>
                {
                    { nameof(PagesRemovedCount), PagesRemovedCount },
                    { durationSeconds, GetDurationSeconds(StartTime, EndTime) },
                    { nameof(OriginalBytes), OriginalBytes },
                    { nameof(Bytes), Bytes },
                    { nameof(PageCount), PageCount }
                }
            );
        }
    }
}