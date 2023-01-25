using System;

namespace coordinator.Domain
{
    public class GetCaseDocumentsActivityPayload : BasePipelinePayload
    {
        public GetCaseDocumentsActivityPayload(string caseUrn, long caseId, string accessToken, string cmsAuthValues, Guid correlationId) :
            base(caseUrn, caseId, correlationId)
        {
            AccessToken = accessToken;
            CmsAuthValues = cmsAuthValues;
        }

        public string AccessToken { get; set; }

        public string CmsAuthValues { get; set; }
    }
}
