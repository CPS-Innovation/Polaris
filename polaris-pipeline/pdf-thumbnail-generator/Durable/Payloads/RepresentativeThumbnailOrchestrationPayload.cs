using System;
using System.ComponentModel.DataAnnotations;

namespace pdf_thumbnail_generator.Durable.Payloads
{
    public class RepresentativeThumbnailOrchestrationPayload
    {
        public RepresentativeThumbnailOrchestrationPayload(string cmsCaseUrn, int cmsCaseId, string documentId, Guid correlationId)
        {
            CmsCaseUrn = cmsCaseUrn;
            CmsCaseId = cmsCaseId;
            CorrelationId = correlationId;
            DocumentId = documentId;
        }

        [Required]
        public string CmsCaseUrn { get; set; }

        [Required]
        public int CmsCaseId { get; set; }

        [Required]
        public Guid CorrelationId { get; set; }
        [Required]
        public string DocumentId { get; set; }
    }
}