﻿namespace Common.Configuration;

public static class RestApi
{
    public const string MaterialTypeList = "reference/reclassification";
    public const string LookupUrn = "urn-lookup/{caseId:min(1)}";
    public const string Cases = "urns/{caseUrn}/cases";
    public const string Case = "urns/{caseUrn}/cases/{caseId:min(1)}";
    public const string CaseTracker = "urns/{caseUrn}/cases/{caseId:min(1)}/tracker";
    public const string CaseSearch = "urns/{caseUrn}/cases/{caseId:min(1)}/search";
    public const string CaseSearchCount = "urns/{caseUrn}/cases/{caseId:min(1)}/search/count";
    public const string CaseExhibitProducers = "urns/{caseUrn}/cases/{caseId:min(1)}/exhibit-producers";
    public const string CaseWitnesses = "urns/{caseUrn}/cases/{caseId:min(1)}/witnesses";
    public const string WitnessStatements = "urns/{caseUrn}/cases/{caseId:min(1)}/witnesses/{witnessId}/statements";
    public const string Documents = "urns/{caseUrn}/cases/{caseId:min(1)}/documents";
    public const string DocumentNotes = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/notes";
    public const string RedactDocument = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/versions/{versionId:min(1)}/redact";
    public const string ModifyDocument = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/versions/{versionId:min(1)}/modify";
    public const string RenameDocument = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/rename";
    public const string ReclassifyDocument = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/reclassify";
    public const string Pdf = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/versions/{versionId:min(1)}/pdf";
    public const string Ocr = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/versions/{versionId:min(1)}/ocr";
    public const string Pii = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/versions/{versionId:min(1)}/pii";
    public const string DocumentCheckout = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/versions/{versionId:min(1)}/checkout";
    public const string ToggleIsUnusedDocument = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/toggle/{isUnused}";
    public const string OcrSearch = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/versions/{versionId:min(1)}/search";

    // Internal Pipeline
    public const string Extract = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/versions/{versionId:min(1)}/extract";
    public const string ConvertToPdf = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/versions/{versionId:min(1)}/convert-to-pdf";
    public const string RemoveCaseIndexes = "urns/{caseUrn}/cases/{caseId:min(1)}/remove-case-indexes";
    public const string CaseIndexCount = "urns/{caseUrn}/cases/{caseId:min(1)}/case-index-count";
    public const string DocumentIndexCount = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/versions/{versionId:min(1)}/document-index-count";
    public const string GenerateThumbnail = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/versions/{versionId}/thumbnails/{maxDimensionPixel}/{pageIndex?}";
    public const string Thumbnail = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/versions/{versionId}/thumbnails/{maxDimensionPixel}/{pageIndex}";
    public const string Status = "status";
    public const string Health = "health";
    public const string GetHostName = "gethostname";

    public static string GetCasePath(string caseUrn, int caseId) => $"urns/{caseUrn}/cases/{caseId}";

    public static string GetCaseTrackerPath(string caseUrn, int caseId) => $"urns/{caseUrn}/cases/{caseId}/tracker";

    public static string GetCaseSearchQueryPath(string caseUrn, int caseId, string searchTerm) =>
        $"urns/{caseUrn}/cases/{caseId}/search?query={searchTerm}";

    public static string GetRedactDocumentPath(string caseUrn, int caseId, string documentId, long versionId) =>
        $"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/redact";

    public static string GetConvertToPdfPath(string caseUrn, int caseId, string documentId, long versionId) =>
        $"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/convert-to-pdf";

    public static string GetExtractPath(string caseUrn, int caseId, string documentId, long versionId) =>
        $"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/extract";

    public static string GetRemoveCaseIndexesPath(string caseUrn, int caseId) => $"urns/{caseUrn}/cases/{caseId}/remove-case-indexes";

    public static string GetSearchPath(string caseUrn, int caseId) => $"urns/{caseUrn}/cases/{caseId}/search";

    public static string GetRedactPdfPath(string caseUrn, int caseId, string documentId, long versionId) =>
        $"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/redact";

    public static string GetCaseIndexCountResultsPath(string caseUrn, int caseId) => $"urns/{caseUrn}/cases/{caseId}/case-index-count";

    public static string GetDocumentIndexCountResultsPath(string caseUrn, int caseId, string documentId, long versionId) =>
        $"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/document-index-count";

    public static string GetModifyDocumentPath(string caseUrn, int caseId, string documentId, long versionId) =>
        $"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/modify";

    public static string CaseSearchCountPath(string caseUrn, int caseId) => $"urns/{caseUrn}/cases/{caseId}/search/count";

    public static string GetThumbnailPath(string caseUrn, int caseId, string documentId, int versionId, int maxDimensionPixel, int? pageIndex) =>
        $"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/thumbnails/{maxDimensionPixel}/{pageIndex}";

    public static string GetBulkRedactionSearchPathAsync(string caseUrn, int caseId, string documentId, long versionId, string searchText) =>
        $"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/search?SearchText={searchText}";
}