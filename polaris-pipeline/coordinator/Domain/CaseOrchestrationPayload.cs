using System;

namespace coordinator.Domain
{
    public class CaseOrchestrationPayload : BasePipelinePayload
    {
        public CaseOrchestrationPayload(string cmsCaseUrn, long cmsCaseId, string cmsAuthValues, Guid correlationId, bool forceRefresh=true)
            : base(default, cmsCaseUrn, cmsCaseId, correlationId)
        {
            ForceRefresh = forceRefresh;
            CmsAuthValues = cmsAuthValues;
        }

        public bool ForceRefresh { get; init; }

        public string AccessToken { get; init; }

        public string CmsAuthValues { get; init; }
    }
}