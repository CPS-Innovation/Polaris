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

        // Documents (plural)
        public const string DocumentsSearch = "urns/{caseUrn}/cases/{caseId}/documents/search";

        // Document (singular)
        public const string Document = "urns/{caseUrn}/cases/{caseId}/documents/{documentId:guid}";
        public const string DocumentCheckout = "urns/{caseUrn}/cases/{caseId}/documents/{documentId:guid}/checkout";
        public const string DocumentSasUrl = "urns/{caseUrn}/cases/{caseId}/documents/{documentId:guid}/sasUrl";

        // Other
        public const string Health = "health";

        public static string GetCaseUrl(string caseUrn, long caseId)
        {
            var url = $"urns/{caseUrn}/cases/{caseId}";
            return url;
        }

        public static string GetDocumentsUrl(string caseUrn, long caseId)
        {
            var url = $"urns/{caseUrn}/cases/{caseId}/documents";
            return url;
        }

        public static string GetDocumentUrl(string caseUrn, long caseId, Guid documentId)
        {
            var url = $"urns/{caseUrn}/cases/{caseId}/documents/{documentId}";
            return url;
        }
    }
}
