using System;

namespace coordinator.Domain
{
    public class GeneratePdfHttpRequestActivityPayload : BasePipelinePayload
    {
        // TODO - move over to PolarisDocumentId
        public GeneratePdfHttpRequestActivityPayload(string caseUrn, long caseId, string documentCategory, string documentId, string fileName, long versionId, string cmsAuthValues, Guid correlationId)
            : base(caseUrn, caseId, correlationId)
        {
            DocumentId = documentId;
            DocumentCategory = documentCategory;
            FileName = fileName;
            VersionId = versionId;
            CmsAuthValues = cmsAuthValues;
        }

        public string DocumentCategory { get; set; }

        public string DocumentId { get; set; }

        public string FileName { get; set; }

        public long VersionId { get; set; }

        public string CmsAuthValues { get; set; }
    }
}
