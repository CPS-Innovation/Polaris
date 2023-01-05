using System;

namespace coordinator.Domain
{
    public class CoordinatorOrchestrationPayload : BasePipelinePayload
    {
        public CoordinatorOrchestrationPayload(string caseUrn, long caseId, bool forceRefresh, string accessToken, string upstreamToken, Guid correlationId)
            : base(caseUrn, caseId, correlationId)
        {
            ForceRefresh = forceRefresh;
            AccessToken = accessToken;
            UpstreamToken = upstreamToken;
        }
        
        public bool ForceRefresh { get; set; }

        public string AccessToken { get; set; }
        
        public string UpstreamToken { get; set; }
    }
}