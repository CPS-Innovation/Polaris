using System;

namespace coordinator.Durable.Payloads
{
    public class CaseOrchestrationPayload : BasePipelinePayload
    {
        public CaseOrchestrationPayload(string cmsCaseUrn, int cmsCaseId, string cmsAuthValues, Guid correlationId)
            : base(cmsCaseUrn, cmsCaseId, correlationId)
        {
            CmsAuthValues = cmsAuthValues;
        }

        public string CmsAuthValues { get; init; }
    }
}