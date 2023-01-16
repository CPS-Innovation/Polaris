using System;

namespace coordinator.Domain
{
    public class GetCaseDocumentsActivityPayload : BasePipelinePayload
    {
        public GetCaseDocumentsActivityPayload(string caseUrn, long caseId, string accessToken, string upstreamToken, Guid correlationId) : 
            base(caseUrn, caseId, correlationId)
        {
            AccessToken = accessToken;
            UpstreamToken = upstreamToken;
        }

        public string AccessToken { get; set; }

        public string UpstreamToken { get; set; }
    }
}
