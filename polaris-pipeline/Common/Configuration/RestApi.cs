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

        // Admin
        public const string ResetDurableState = "maintenance/resetDurableState";

        // Other
        public const string AuthInitialisation = "init";
        public const string Status = "status";
        public const string GetHostName = "gethostname";

        // Internal Pipeline
        public const string Search = "urns/{caseUrn}/cases/{caseId}/search";
        public const string Extract = "urns/{caseUrn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/extract";
        public const string ConvertToPdf = "urns/{caseUrn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/convert-to-pdf";
        public const string RedactPdf = "urns/{caseUrn}/cases/{caseId}/documents/{documentId}/redact-pdf";
        public const string RemoveCaseIndexes = "urns/{caseUrn}/cases/{caseId}/remove-case-indexes";
        public const string WaitForCaseEmptyResults = "urns/{caseUrn}/cases/{caseId}/wait-for-case-empty-results";

#if SCALABILITY_TEST
        public const string ScalabilityTest = "cases/{caseId}/documents/{documentCount}/scalability-test";
        public const string ScalabilityTestTracker = "cases/{caseId}/scalability-test/tracker";
#endif

        public static string GetCasePath(string caseUrn, long caseId)
        {
            var url = $"urns/{caseUrn}/cases/{caseId}";
            return url;
        }

        public static string GetCaseTrackerPath(string caseUrn, long caseId)
        {
            var url = $"urns/{caseUrn}/cases/{caseId}/tracker";
            return url;
        }

        public static string GetCaseSearchPath(string caseUrn, long caseId)
        {
            var url = $"urns/{caseUrn}/cases/{caseId}/search";
            return url;
        }

        public static string GetDocumentsPath(string caseUrn, long caseId)
        {
            var url = $"urns/{caseUrn}/cases/{caseId}/documents";
            return url;
        }

        public static string GetDocumentPath(string caseUrn, long caseId, PolarisDocumentId polarisDocumentId)
        {
            var url = $"urns/{caseUrn}/cases/{caseId}/documents/{polarisDocumentId}";
            return url;
        }

        public static string GetInstancePath(string instanceId)
        {
            var url = $"runtime/webhooks/durabletask/instances/{instanceId}";
            return url;
        }

        public static string GetDurableEntityPath(string durableEntityType, string instanceId)
        {
            var url = $"runtime/webhooks/durabletask/entities/{durableEntityType}/{instanceId}";
            return url;
        }

        public static string GetInstancesPath()
        {
            return "runtime/webhooks/durabletask/instances";
        }

        public static string GetConvertToPdfPath(string caseUrn, string caseId, string documentId, string versionId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/convert-to-pdf";
        }

        public static string GetExtractPath(string caseUrn, long caseId, string documentId, long versionId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/extract";
        }

        public static string GetRemoveCaseIndexesPath(string caseUrn, long caseId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/remove-case-indexes";
        }

        public static string GetWaitForCaseEmptyResultsPath(string caseUrn, long caseId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/wait-for-case-empty-results";
        }

        public static string GetSearchPath(string caseUrn, long caseId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/search";
        }

        public static string GetRedactPdfPath(string caseUrn, string caseId, string documentId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/redact-pdf";
        }
    }
}
