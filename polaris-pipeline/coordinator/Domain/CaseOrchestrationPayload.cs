using System;

namespace coordinator.Domain
{
    public class CaseOrchestrationPayload : BasePipelinePayload
    {
        public CaseOrchestrationPayload(string cmsCaseUrn, long cmsCaseId, string cmsAuthValues, Guid correlationId)
            : base(cmsCaseUrn, cmsCaseId, correlationId)
        {
            CmsAuthValues = cmsAuthValues;
        }

        public string AccessToken { get; init; }

        public string CmsAuthValues { get; init; }
    }
}