using Common.ValueObjects;
using coordinator.Durable.Payloads;
using System;

namespace coordinator.Domain
{
    public class TextExtractorHttpRequestActivityPayload : BasePipelinePayload
    {
        public TextExtractorHttpRequestActivityPayload(PolarisDocumentId polarisDocumentId, string cmsCaseUrn, long cmsCaseId, string cmsDocumentId, long cmsVersionId, string blobName, Guid correlationId)
            : base(cmsCaseUrn, cmsCaseId, correlationId, polarisDocumentId)
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
