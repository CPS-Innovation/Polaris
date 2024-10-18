using System;

namespace Common.Configuration
{
    public static class RestApi
    {
        public const string LookupUrn = "urn-lookup/{caseId:min(1)}";

        // Cases (plural)
        public const string Cases = "urns/{caseUrn}/cases";

        // Case (singular)
        public const string Case = "urns/{caseUrn}/cases/{caseId:min(1)}";
        public const string CaseTracker = "urns/{caseUrn}/cases/{caseId:min(1)}/tracker";
        public const string CaseSearch = "urns/{caseUrn}/cases/{caseId:min(1)}/search";
        public const string CaseSearchCount = "urns/{caseUrn}/cases/{caseId:min(1)}/search/count";
        public const string CaseExhibitProducers = "urns/{caseUrn}/cases/{caseId:min(1)}/exhibit-producers";
        public const string CaseWitnesses = "urns/{caseUrn}/cases/{caseId:min(1)}/witnesses";
        public const string WitnessStatements = "urns/{caseUrn}/cases/{caseId:min(1)}/witnesses/{witnessId}/statements";
        public const string Documents = "urns/{caseUrn}/cases/{caseId:min(1)}/documents";
        public const string Pdf = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/versions/{versionId}/pdf";
        public const string Ocr = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/versions/{versionId}/ocr";
        public const string Pii = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/versions/{versionId}/pii";
        // Document (singular)
        public const string Document = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}";
        public const string DocumentCheckout = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/checkout";
        public const string DocumentNotes = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/notes";
        public const string RedactDocument = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/redact";
        public const string ModifyDocument = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/modify";
        public const string RenameDocument = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/rename";
        public const string ReclassifyDocument = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/reclassify";

        // Reference
        public const string MaterialTypeList = "reference/reclassification";

        // Other
        public const string AuthInitialisation = "init";
        public const string Status = "status";
        public const string Health = "health";
        public const string GetHostName = "gethostname";

        // Internal Pipeline
        public const string Search = "urns/{caseUrn}/cases/{caseId:min(1)}/search";
        public const string Extract = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/versions/{versionId}/extract";
        public const string ConvertToPdf = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/versions/{versionId}/convert-to-pdf";
        public const string RedactPdf = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/redact-pdf";

        public const string RemoveCaseIndexes = "urns/{caseUrn}/cases/{caseId:min(1)}/remove-case-indexes";
        public const string CaseIndexCount = "urns/{caseUrn}/cases/{caseId:min(1)}/case-index-count";
        public const string DocumentIndexCount = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/versions/{versionId}/document-index-count";

        public static string GetCasePath(string caseUrn, int caseId)
        {
            return $"urns/{caseUrn}/cases/{caseId}";
        }

        public static string GetCaseTrackerPath(string caseUrn, int caseId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/tracker";
        }

        public static string GetCaseSearchQueryPath(string caseUrn, int caseId, string searchTerm)
        {
            return $"urns/{caseUrn}/cases/{caseId}/search?query={searchTerm}";
        }

        public static string GetRedactDocumentPath(string caseUrn, int caseId, string documentId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/redact";
        }

        public static string GetDocumentCheckoutPath(string caseUrn, int caseId, string documentId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/checkout";
        }

        public static string GetDocumentNotesPath(string caseUrn, int caseId, string documentId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/notes";
        }

        public static string GetConvertToPdfPath(string caseUrn, int caseId, string documentId, long versionId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/convert-to-pdf";
        }

        public static string GetExtractPath(string caseUrn, int caseId, string documentId, long versionId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/extract";
        }

        public static string GetRemoveCaseIndexesPath(string caseUrn, int caseId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/remove-case-indexes";
        }

        public static string GetSearchPath(string caseUrn, int caseId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/search";
        }

        public static string GetRedactPdfPath(string caseUrn, int caseId, string documentId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/redact-pdf";
        }

        public static string GetRenameDocumentPath(string caseUrn, int caseId, string documentId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/rename";
        }

        public static string GetCaseIndexCountResultsPath(string caseUrn, int caseId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/case-index-count";
        }

        public static string GetDocumentIndexCountResultsPath(string caseUrn, int caseId, string documentId, long versionId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/document-index-count";
        }

        public static string GetPiiPath(string caseUrn, int caseId, string documentId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/pii";
        }

        public static string GetModifyDocumentPath(string caseUrn, int caseId, string documentId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/modify";
        }

        public static string CaseSearchCountPath(string caseUrn, int caseId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/search/count";
        }

        public static string GetReclassifyDocumentPath(string caseUrn, int caseId, string documentId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/reclassify";
        }

        public static string GetCaseExhibitProducersPath(string caseUrn, int caseId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/exhibit-producers";
        }

        public static string GetCaseWitnessesPath(string caseUrn, int caseId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/witnesses";
        }

        public static string GetWitnessStatementsPath(string caseUrn, int caseId, int witnessId)
        {
            return $"urns/{caseUrn}/cases/{caseId}/witnesses/{witnessId}/statements";
        }
    }
}
