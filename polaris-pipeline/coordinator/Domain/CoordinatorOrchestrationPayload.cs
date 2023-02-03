using System;

namespace coordinator.Domain
{
    public class CoordinatorOrchestrationPayload : BasePipelinePayload
    {
        public CoordinatorOrchestrationPayload(string caseUrn, long caseId, bool forceRefresh, string accessToken, string cmsAuthValues, Guid correlationId)
            : base(caseUrn, caseId, correlationId)
        {
            ForceRefresh = forceRefresh;
            AccessToken = accessToken;
            CmsAuthValues = cmsAuthValues;
        }

        public bool ForceRefresh { get; set; }

        public string AccessToken { get; set; }

        public string CmsAuthValues { get; set; }
    }
}