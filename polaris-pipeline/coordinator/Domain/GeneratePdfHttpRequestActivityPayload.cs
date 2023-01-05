using System;

namespace coordinator.Domain
{
    public class GeneratePdfHttpRequestActivityPayload : BasePipelinePayload
    {
        public GeneratePdfHttpRequestActivityPayload(string caseUrn, long caseId, string documentCategory, string documentId, string fileName, long versionId, string upstreamToken, Guid correlationId)
            : base(caseUrn, caseId, correlationId)
        {
            DocumentId = documentId;
            DocumentCategory = documentCategory;
            FileName = fileName;
            VersionId = versionId;
            UpstreamToken = upstreamToken;
        }

        public string DocumentCategory { get; set; }
        
        public string DocumentId { get; set; }

        public string FileName { get; set; }

        public long VersionId { get; set; }

        public string UpstreamToken { get; set; }
    }
}
