using System.ComponentModel.DataAnnotations;

namespace pdf_thumbnail_generator.Durable.Payloads
{
    public class ThumbnailOrchestrationPayload
    {
        public ThumbnailOrchestrationPayload(string urn, int caseId, string documentId, long versionId, int maxDimensionPixel, Guid correlationId, int? pageIndex)
        {
            Urn = urn;
            CaseId = caseId;
            CorrelationId = correlationId;
            DocumentId = documentId;
            VersionId = versionId;
            PageIndex = pageIndex;
            MaxDimensionPixel = maxDimensionPixel;
        }

        [Required]
        public string Urn { get; set; }

        [Required]
        public int CaseId { get; set; }

        [Required]
        public Guid CorrelationId { get; set; }
        [Required]
        public string DocumentId { get; set; }
        [Required]
        public long VersionId { get; set; }
        [Required]
        public int MaxDimensionPixel { get; set; }
        public int? PageIndex { get; set; }
    }
}