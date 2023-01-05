using System;

namespace coordinator.Domain
{
    public class GetCaseDocumentsActivityPayload : BasePipelinePayload
    {
        public GetCaseDocumentsActivityPayload(string caseUrn, long caseId, string upstreamToken, Guid correlationId) : 
            base(caseUrn, caseId, correlationId)
        {
            UpstreamToken = upstreamToken;
        }

        public string UpstreamToken { get; set; }
    }
}
