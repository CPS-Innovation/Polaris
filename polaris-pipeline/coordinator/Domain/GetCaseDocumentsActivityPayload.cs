using System;

namespace coordinator.Domain
{
    public class GetCaseDocumentsActivityPayload : BasePipelinePayload
    {
        // TODO - move over to PolarisDocumentId
        public GetCaseDocumentsActivityPayload(string caseUrn, long caseId, string cmsAuthValues, Guid correlationId) :
            base(default, caseUrn, caseId, correlationId)
        {
            CmsAuthValues = cmsAuthValues;
        }

        public string CmsAuthValues { get; set; }
    }
}
