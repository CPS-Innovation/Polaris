using System;

namespace coordinator.Domain
{
    public class CoordinatorOrchestrationPayload : BasePipelinePayload
    {
        public CoordinatorOrchestrationPayload(string cmsCaseUrn, long cmsCaseId, bool forceRefresh, string cmsAuthValues, Guid correlationId)
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