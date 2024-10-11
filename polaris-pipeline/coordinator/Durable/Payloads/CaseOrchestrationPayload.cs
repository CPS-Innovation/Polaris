using System;

namespace coordinator.Durable.Payloads
{
    public class CaseOrchestrationPayload : BasePipelinePayload
    {
        public CaseOrchestrationPayload(string urn, int caseId, string cmsAuthValues, Guid correlationId)
            : base(urn, caseId, correlationId)
        {
            CmsAuthValues = cmsAuthValues;
        }

        public string CmsAuthValues { get; init; }
    }
}