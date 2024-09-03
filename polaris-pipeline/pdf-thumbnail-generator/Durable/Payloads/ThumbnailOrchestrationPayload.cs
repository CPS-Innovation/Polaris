using System;
using System.ComponentModel.DataAnnotations;

namespace pdf_thumbnail_generator.Durable.Payloads
{
    public class ThumbnailOrchestrationPayload
    {
        public ThumbnailOrchestrationPayload(string cmsCaseUrn, int cmsCaseId, string documentId, int versionId, int maxDimensionPixel, Guid correlationId, int? pageIndex)
        {
            CmsCaseUrn = cmsCaseUrn;
            CmsCaseId = cmsCaseId;
            CorrelationId = correlationId;
            DocumentId = documentId;
            VersionId = versionId;
            PageIndex = pageIndex;
            MaxDimensionPixel = maxDimensionPixel;
        }

        [Required]
        public string CmsCaseUrn { get; set; }

        [Required]
        public int CmsCaseId { get; set; }

        [Required]
        public Guid CorrelationId { get; set; }
        [Required]
        public string DocumentId { get; set; }
        [Required]
        public int VersionId { get; set; }
        [Required]
        public int MaxDimensionPixel { get; set; }
        public int? PageIndex { get; set; }
    }
}