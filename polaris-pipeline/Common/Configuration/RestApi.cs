using Common.ValueObjects;

namespace Common.Configuration
{
    public static class RestApi
    {
        // Cases (plural)
        public const string Cases = "urns/{caseUrn}/cases";

        // Case (singular)
        public const string Case = "urns/{caseUrn}/cases/{caseId:min(1)}";
        public const string CaseTracker = "urns/{caseUrn}/cases/{caseId:min(1)}/tracker";
        public const string CaseSearch = "urns/{caseUrn}/cases/{caseId:min(1)}/search";

        // Document (singular)
        public const string Document = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{polarisDocumentId}";
        public const string DocumentCheckout = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{polarisDocumentId}/checkout";
        public const string RedactDocument = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{polarisDocumentId}/redact";

        // Other
        public const string AuthInitialisation = "init";
        public const string Status = "status";
        public const string GetHostName = "gethostname";

        // Internal Pipeline
        public const string Search = "urns/{caseUrn}/cases/{caseId:min(1)}/search";
        public const string Extract = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/versions/{versionId}/extract";
        public const string ConvertToPdf = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/versions/{versionId}/convert-to-pdf";
        public const string RedactPdf = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/redact-pdf";
        public const string PdfRedactor = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/pdf-redactor";
        public const string RemoveCaseIndexes = "urns/{caseUrn}/cases/{caseId:min(1)}/remove-case-indexes";
        public const string WaitForCaseEmptyResults = "urns/{caseUrn}/cases/{caseId:min(1)}/wait-for-case-empty-results";
        public const string CaseIndexCount = "urns/{caseUrn}/cases/{caseId:min(1)}/case-index-count";
        public const string DocumentIndexCount = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/versions/{versionId}/document-index-count";

        public static string GetCasesPath(string caseUrn)
        {
            return $"urns/{caseUrn}/cases";
        }

        public static string GetCasePath(string caseUrn, long caseId)
        {
            return $"urns/{caseUrn}/cases/{caseId}";
        }

        public static string GetCaseTrackerPath(string caseUrn, long caseId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/tracker";
        }

        public static string GetCaseSearchQueryPath(string caseUrn, long caseId, string searchTerm)
        {
            return $"urns/{caseUrn}/cases/{caseId}/search?query={searchTerm}";
        }

        // todo: delete following
        public static string GetCaseSearchPath(string caseUrn, long caseId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/search";
        }

        public static string GetDocumentPath(string caseUrn, long caseId, PolarisDocumentId polarisDocumentId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/documents/{polarisDocumentId}";
        }
        public static string RedactDocumentPath(string caseUrn, long caseId, PolarisDocumentId polarisDocumentId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/documents/{polarisDocumentId}/redact";
        }

        public static string GetDocumentCheckoutPath(string caseUrn, long caseId, PolarisDocumentId polarisDocumentId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/documents/{polarisDocumentId}/checkout";
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

        public static string GetPdfRedactorPath(string caseUrn, string caseId, string documentId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/pdf-redactor";
        }

        public static string GetCaseIndexCountResultsPath(string caseUrn, long caseId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/case-index-count";
        }

        public static string GetDocumentIndexCountResultsPath(string caseUrn, long caseId, string documentId, long versionId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/document-index-count";
        }
    }
}
