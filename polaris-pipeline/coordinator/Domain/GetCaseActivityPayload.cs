using System;

namespace coordinator.Domain
{
    public class GetCaseActivityPayload : BasePipelinePayload
    {
        public GetCaseActivityPayload(string caseUrn, long caseId, string cmsAuthValues, Guid correlationId) :
            base(caseUrn, caseId, correlationId)
        {
            CmsAuthValues = cmsAuthValues;
        }

        public string CmsAuthValues { get; set; }
    }
}
