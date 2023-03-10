using System;

namespace coordinator.Domain
{
    public class CaseDocumentOrchestrationPayload : BasePipelinePayload
    {
        public CaseDocumentOrchestrationPayload
            (
                Guid polarisDocumentId,
                string cmsCaseUrn, 
                long cmsCaseId, 
                string cmsDocumentCategory, 
                string cmsDocumentId, 
                long cmsVersionId, 
                string cmsFileName, 
                string cmsAuthValues, 
                Guid correlationId
            )
            : base(polarisDocumentId, cmsCaseUrn, cmsCaseId, correlationId)
        {
            CmsDocumentCategory = cmsDocumentCategory;
            CmsDocumentId = cmsDocumentId;
            CmsVersionId = cmsVersionId;
            CmsFileName = cmsFileName;
            CmsAuthValues = cmsAuthValues;
        }

        public string CmsDocumentCategory { get; set; }

        public string CmsDocumentId { get; set; }

        public long CmsVersionId { get; set; }

        public string CmsFileName { get; set; }

        public string CmsAuthValues { get; set; }
    }
}
