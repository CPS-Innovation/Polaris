using System;

namespace coordinator.Domain
{
    public class SearchCasePayload
    {
        public SearchCasePayload(int cmsCaseId, string query, string correlationId)
        {
            CmsCaseId = cmsCaseId;
            Query = query;
            CorrelationId = correlationId;
        }

        public int CmsCaseId { get; set; }
        public string Query { get; set; }
        public string CorrelationId { get; set; }
    }
}
