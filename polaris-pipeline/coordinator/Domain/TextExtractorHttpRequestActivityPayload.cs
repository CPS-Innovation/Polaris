using System;

namespace coordinator.Domain
{
    public class TextExtractorHttpRequestActivityPayload : BasePipelinePayload
    {
        // TODO - move over to PolarisDocumentId
        public TextExtractorHttpRequestActivityPayload(Guid polarisDocumentId, string cmsCaseUrn, long cmsCaseId, string cmsDocumentId, long cmsVersionId, string blobName, Guid correlationId)
            : base(polarisDocumentId, cmsCaseUrn, cmsCaseId, correlationId)
        {
            DocumentId = cmsDocumentId;
            VersionId = cmsVersionId;
            BlobName = blobName;
        }
        
        public string DocumentId { get; set; }

        public long VersionId { get; set; }

        public string BlobName { get; set; }
    }
}
