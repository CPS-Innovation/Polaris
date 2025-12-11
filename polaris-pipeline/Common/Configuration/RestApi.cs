using System.Security.AccessControl;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Common.Configuration;

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
    public const string OcrSearchTracker = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/versions/{versionId:min(1)}/search/tracker";

    // House keeping endpoints
    public const string CaseInfo = "urns/{caseUrn}/case-info/{caseId:min(1)}";
    public const string CaseMaterials = "urns/{caseUrn}/cases/{caseId:min(1)}/case-materials";
    public const string CaseMaterialsPreview = "urns/{caseUrn}/cases/{caseId:min(1)}/materials/{materialId}/preview";
    public const string MaterialDocument = "urns/{caseUrn}/cases/{caseId:min(1)}/materials/{materialId}/document";
    public const string DocumentTypes = "urns/{caseUrn}/cases/{caseId:min(1)}/document-types";
    public const string ExhibitProducers = "urns/{caseUrn}/cases/{caseId:min(1)}/case-exhibit-producers";
    public const string CaseWitnessStatements = "urns/{caseUrn}/cases/{caseId:min(1)}/witnesses/{witnessId}/witness-statements";
    public const string CompleteReclassification = "urns/{caseUrn}/cases/{caseId:min(1)}/materials/{materialId}/reclassify-complete";
    public const string CaseWitnessesHk = "urns/{caseUrn}/cases/{caseId:min(1)}/case-witnesses";
    public const string CaseLockInfo = "urns/{caseUrn}/cases/{caseId:min(1)}/case-lock-info";
    public const string RenameMaterial = "urns/{caseUrn}/cases/{caseId:min(1)}/materials/{materialId}/rename";
    public const string DiscardMaterial = "urns/{caseUrn}/cases/{caseId:min(1)}/materials/{materialId}/discard";
    public const string PcdRequest = "urns/{caseUrn}/cases/{caseId:min(1)}/pcds/{pcdId}/pcd-request";
    public const string PcdRequestCore = "urns/{caseUrn}/cases/{caseId:min(1)}/pcds/{pcdId}/pcd-request-core";
    public const string CaseDefendants = "urns/{caseUrn}/cases/{caseId:min(1)}/case-defendants";
    public const string UpdateExhibit = "urns/{caseUrn}/cases/{caseId:min(1)}/materials/{materialId}/exhibit";
    public const string UpdateStatement = "urns/{caseUrn}/cases/{caseId:min(1)}/materials/{materialId}/statement";
    public const string CaseHistoryEvent = "urns/{caseUrn}/cases/{caseId:min(1)}/history";
    public const string InitialReviewByHistoryId = "urns/{caseUrn}/cases/{caseId:min(1)}/history/{historyId}/initial-review";
    public const string InitialReviewByCase = "urns/{caseUrn}/cases/{caseId:min(1)}/initial-review";
    public const string OffenseCharge = "urns/{caseUrn}/cases/{caseId:min(1)}/history/{historyId}/offence-charge";
    public const string PreChargeDecision = "urns/{caseUrn}/cases/{caseId:min(1)}/pre-charge-decision";
    public const string PreChargeDecisionByHistoryId = "urns/{caseUrn}/cases/{caseId:min(1)}/history/{historyId}/pre-charge-decision";

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
    public static string GetBulkRedactionSearchTrackerPath(string caseUrn, int caseId, string documentId, long versionId, string searchText) => $"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/search/tracker?SearchText={searchText}";

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