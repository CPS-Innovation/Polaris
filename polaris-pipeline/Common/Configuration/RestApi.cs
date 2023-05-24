using Common.ValueObjects;
using System;

namespace Common.Configuration
{
    public static class RestApi
    {
        // Cases (plural)
        public const string Cases = "urns/{caseUrn}/cases";

        // Case (singular)
        public const string Case = "urns/{caseUrn}/cases/{caseId}";
        public const string CaseTracker = "urns/{caseUrn}/cases/{caseId}/tracker";
        public const string CaseSearch = "urns/{caseUrn}/cases/{caseId}/search";

        // Document (singular)
        public const string Document = "urns/{caseUrn}/cases/{caseId}/documents/{polarisDocumentId}";
        public const string DocumentCheckout = "urns/{caseUrn}/cases/{caseId}/documents/{polarisDocumentId}/checkout";
        public const string DocumentSasUrl = "urns/{caseUrn}/cases/{caseId}/documents/{polarisDocumentId}/sasUrl";
        
        // Admin
        public const string ResetDurableState = "maintenance/resetDurableState";

        // Other
        public const string Health = "health";

        public static string GetCaseUrl(string caseUrn, long caseId)
        {
            var url = $"urns/{caseUrn}/cases/{caseId}";
            return url;
        }

        public static string GetCaseOrchestrationInstanceUrl(string instanceId)
        {
            var url = $"runtime/webhooks/durabletask/instances/{instanceId}";
            return url;
        }


        public static string GetDocumentsUrl(string caseUrn, long caseId)
        {
            var url = $"urns/{caseUrn}/cases/{caseId}/documents";
            return url;
        }

        public static string GetDocumentUrl(string caseUrn, long caseId, PolarisDocumentId polarisDocumentId)
        {
            var url = $"urns/{caseUrn}/cases/{caseId}/documents/{polarisDocumentId}";
            return url;
        }
    }
}
