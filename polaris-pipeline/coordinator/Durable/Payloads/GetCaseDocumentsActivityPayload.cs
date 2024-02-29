using System;

namespace coordinator.Durable.Payloads
{
    public class GetCaseDocumentsActivityPayload : BasePipelinePayload
    {
        public GetCaseDocumentsActivityPayload(string caseUrn, long caseId, string cmsAuthValues, Guid correlationId) :
            base(caseUrn, caseId, correlationId)
        {
            CmsAuthValues = cmsAuthValues;
        }

        public string CmsAuthValues { get; set; }
    }
}
