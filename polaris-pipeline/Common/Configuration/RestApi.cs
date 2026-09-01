namespace Common.Configuration;

public static class RestApi
{
    public const string MaterialTypeList = "reference/reclassification";
    public const string LookupUrn = "urn-lookup/{caseId:min(1)}";
    public const string Cases = "urns/{caseUrn}/cases";
    public const string CaseLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}";
    public const string Case = "cases/{caseId:min(1)}";
    public const string CaseTrackerLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/tracker";
    public const string CaseTracker = "cases/{caseId:min(1)}/tracker";
    public const string CaseSearchLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/search";
    public const string CaseSearch = "cases/{caseId:min(1)}/search";
    public const string CaseSearchCountLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/search/count";
    public const string CaseSearchCount = "cases/{caseId:min(1)}/search/count";
    public const string CaseExhibitProducersLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/exhibit-producers";
    public const string CaseExhibitProducers = "cases/{caseId:min(1)}/exhibit-producers";
    public const string CaseWitnessesLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/witnesses";
    public const string CaseWitnesses = "cases/{caseId:min(1)}/witnesses";
    public const string WitnessStatementsLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/witnesses/{witnessId}/statements";
    public const string WitnessStatements = "cases/{caseId:min(1)}/witnesses/{witnessId}/statements";
    public const string DocumentsLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/documents";
    public const string Documents = "cases/{caseId:min(1)}/documents";
    public const string DocumentNotesLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{materialId}/notes";
    public const string DocumentNotes = "cases/{caseId:min(1)}/materials/{materialId}/notes";
    public const string RedactDocumentLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{materialId}/versions/{documentId:min(1)}/redact";
    public const string RedactDocument = "cases/{caseId:min(1)}/materials/{materialId}/documents/{documentId:min(1)}/redact";
    public const string RedactPdf = "cases/{caseId:min(1)}/materials/{materialId}/redact";
    public const string RedactAndLog = "cases/{caseId:min(1)}/documents/{materialId}/versions/{documentId:min(1)}/redactandlog";
    public const string ModifyDocumentLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{materialId}/versions/{documentId:min(1)}/modify";
    public const string ModifyDocument = "cases/{caseId:min(1)}/materials/{materialId}/documents/{documentId:min(1)}/modify";
    public const string RenameDocumentLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{materialId}/rename";
    public const string RenameDocument = "cases/{caseId:min(1)}/materials/{materialId}/rename";
    public const string ReclassifyDocumentLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{materialId}/reclassify";
    public const string ReclassifyDocument = "cases/{caseId:min(1)}/materials/{materialId}/reclassify";
    public const string PdfLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{materialId}/versions/{documentId:min(1)}/pdf";
    public const string Pdf = "cases/{caseId:min(1)}/materials/{materialId}/documents/{documentId:min(1)}/pdf";
    public const string OcrLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{materialId}/versions/{documentId:min(1)}/ocr";
    public const string Ocr = "cases/{caseId:min(1)}/materials/{materialId}/documents/{documentId:min(1)}/ocr";
    public const string PiiLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{materialId}/versions/{documentId:min(1)}/pii";
    public const string Pii = "cases/{caseId:min(1)}/materials/{materialId}/documents/{documentId:min(1)}/pii";
    public const string DocumentCheckoutLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{materialId}/versions/{documentId:min(1)}/checkout";
    public const string DocumentCheckout = "cases/{caseId:min(1)}/materials/{materialId}/documents/{documentId:min(1)}/checkout";
    public const string ToggleIsUnusedDocumentLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{materialId}/toggle/{isUnused}";
    public const string ToggleIsUnusedDocument = "cases/{caseId:min(1)}/materials/{materialId}/toggle/{isUnused}";
    public const string OcrSearch = "cases/{caseId:min(1)}/materials/{materialId}/documents/{documentId:min(1)}/search";
    public const string OcrSearchTracker = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{materialId}/versions/{documentId:min(1)}/search/tracker";

    // House keeping endpoints (legacy, URN-based - retained for existing system compatibility)
    public const string CaseInfoLegacy = "urns/{caseUrn}/case-info/{caseId:min(1)}";
    public const string CaseMaterialsLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/case-materials";
    public const string CaseMaterialsPreviewLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/materials/{materialId}/preview";
    public const string MaterialDocumentLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/materials/{materialId}/document";
    public const string DocumentTypesLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/document-types";
    public const string ExhibitProducersLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/case-exhibit-producers";
    public const string CaseWitnessStatementsLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/witnesses/{witnessId}/witness-statements";
    public const string CompleteReclassificationLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/materials/{materialId}/reclassify-complete";
    public const string CaseWitnessesHkLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/case-witnesses";
    public const string CaseLockInfoLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/case-lock-info";
    public const string RenameMaterialLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/materials/{materialId}/rename";
    public const string DiscardMaterialLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/materials/{materialId}/discard";
    public const string PcdRequestLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/pcds/{pcdId}/pcd-request";
    public const string PcdRequestCoreLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/pcds/{pcdId}/pcd-request-core";
    public const string CaseDefendantsLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/case-defendants";
    public const string UpdateExhibitLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/materials/{materialId}/exhibit";
    public const string UpdateStatementLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/materials/{materialId}/statement";
    public const string CaseHistoryEventLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/history";
    public const string InitialReviewByHistoryIdLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/history/{historyId}/initial-review";
    public const string InitialReviewByCaseLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/initial-review";
    public const string OffenseChargeLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/history/{historyId}/offence-charge";
    public const string PreChargeDecisionLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/pre-charge-decision";
    public const string PreChargeDecisionByHistoryIdLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/history/{historyId}/pre-charge-decision";
    public const string PcdReviewLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/pcd-review";
    public const string PcdReviewCoreLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/pcd-review-core";
    public const string PcdReviewDetailsLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/history/{historyId}/pcd-review-details";
    public const string ReadStatusLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/materials/{materialId}/read-status";
    public const string UmaReclassifyLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/uma-reclassify";
    public const string BulkSetUnusedLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/bulk-set-unused";

    // House keeping endpoints (new, no URN in route)
    public const string CaseInfo = "case-info/{caseId:min(1)}";
    public const string CaseMaterials = "cases/{caseId:min(1)}/case-materials";
    public const string CaseMaterialsPreview = "cases/{caseId:min(1)}/materials/{materialId}/preview";
    public const string MaterialDocument = "cases/{caseId:min(1)}/materials/{materialId}/document";
    public const string DocumentTypes = "cases/{caseId:min(1)}/document-types";
    public const string ExhibitProducers = "cases/{caseId:min(1)}/case-exhibit-producers";
    public const string CaseWitnessStatements = "cases/{caseId:min(1)}/witnesses/{witnessId}/witness-statements";
    public const string CompleteReclassification = "cases/{caseId:min(1)}/materials/{materialId}/reclassify-complete";
    public const string CaseWitnessesHk = "cases/{caseId:min(1)}/case-witnesses";
    public const string CaseLockInfo = "cases/{caseId:min(1)}/case-lock-info";
    public const string RenameMaterial = "cases/{caseId:min(1)}/materials/{materialId}/rename";
    public const string DiscardMaterial = "cases/{caseId:min(1)}/materials/{materialId}/discard";
    public const string PcdRequest = "cases/{caseId:min(1)}/pcds/{pcdId}/pcd-request";
    public const string PcdRequestCore = "cases/{caseId:min(1)}/pcds/{pcdId}/pcd-request-core";
    public const string CaseDefendants = "cases/{caseId:min(1)}/case-defendants";
    public const string UpdateExhibit = "cases/{caseId:min(1)}/materials/{materialId}/exhibit";
    public const string UpdateStatement = "cases/{caseId:min(1)}/materials/{materialId}/statement";
    public const string CaseHistoryEvent = "cases/{caseId:min(1)}/history";
    public const string InitialReviewByHistoryId = "cases/{caseId:min(1)}/history/{historyId}/initial-review";
    public const string InitialReviewByCase = "cases/{caseId:min(1)}/initial-review";
    public const string OffenseCharge = "cases/{caseId:min(1)}/history/{historyId}/offence-charge";
    public const string PreChargeDecision = "cases/{caseId:min(1)}/pre-charge-decision";
    public const string PreChargeDecisionByHistoryId = "cases/{caseId:min(1)}/history/{historyId}/pre-charge-decision";
    public const string PcdReview = "cases/{caseId:min(1)}/pcd-review";
    public const string PcdReviewCore = "cases/{caseId:min(1)}/pcd-review-core";
    public const string PcdReviewDetails = "cases/{caseId:min(1)}/history/{historyId}/pcd-review-details";
    public const string ReadStatus = "cases/{caseId:min(1)}/materials/{materialId}/read-status";
    public const string UmaReclassify = "cases/{caseId:min(1)}/uma-reclassify";
    public const string BulkSetUnused = "cases/{caseId:min(1)}/bulk-set-unused";
    // Internal Pipeline
    public const string ExtractLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{materialId}/versions/{documentId:min(1)}/extract";
    public const string Extract = "cases/{caseId:min(1)}/materials/{materialId}/documents/{documentId:min(1)}/extract";
    public const string ConvertToPdf = "cases/{caseId:min(1)}/materials/{materialId}/documents/{documentId:min(1)}/convert-to-pdf";
    public const string ConvertToPdfLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{materialId}/versions/{documentId:min(1)}/convert-to-pdf";
    public const string RemoveCaseIndexesLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/remove-case-indexes";
    public const string RemoveCaseIndexes = "cases/{caseId:min(1)}/remove-case-indexes";
    public const string CaseIndexCountLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/case-index-count";
    public const string CaseIndexCount = "cases/{caseId:min(1)}/case-index-count";
    public const string DocumentIndexCountLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{materialId}/versions/{documentId:min(1)}/document-index-count";
    public const string DocumentIndexCount = "cases/{caseId:min(1)}/materials/{materialId}/documents/{documentId:min(1)}/document-index-count";
    public const string GenerateThumbnailLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{materialId}/versions/{documentId}/thumbnails/{maxDimensionPixel}/{pageIndex?}";
    public const string GenerateThumbnail = "cases/{caseId:min(1)}/materials/{materialId}/documents/{documentId}/thumbnails/{maxDimensionPixel}/{pageIndex?}";
    public const string ThumbnailLegacy = "urns/{caseUrn}/cases/{caseId:min(1)}/documents/{materialId}/versions/{documentId}/thumbnails/{maxDimensionPixel}/{pageIndex}";
    public const string Thumbnail = "cases/{caseId:min(1)}/materials/{materialId}/documents/{documentId}/thumbnails/{maxDimensionPixel}/{pageIndex}";
    public const string Status = "status";
    public const string Health = "health";
    public const string GetHostName = "gethostname";

    public static string GetCasePath(string caseUrn, int caseId) => $"urns/{caseUrn}/cases/{caseId}";

    public static string GetCaseTrackerPath(string caseUrn, int caseId) => $"urns/{caseUrn}/cases/{caseId}/tracker";
    public static string GetBulkRedactionSearchTrackerPath(string caseUrn, int caseId, string materialId, long documentId, string searchText) => $"urns/{caseUrn}/cases/{caseId}/documents/{materialId}/versions/{documentId}/search/tracker?SearchText={searchText}";

    public static string GetCaseSearchQueryPath(string caseUrn, int caseId, string searchTerm) =>
        $"urns/{caseUrn}/cases/{caseId}/search?query={searchTerm}";

    public static string GetRedactDocumentPath(string caseUrn, int caseId, string materialId, long documentId) =>
        $"urns/{caseUrn}/cases/{caseId}/documents/{materialId}/versions/{documentId}/redact";

    public static string GetConvertToPdfPath(string caseUrn, int caseId, string materialId, long documentId) =>
        $"urns/{caseUrn}/cases/{caseId}/documents/{materialId}/versions/{documentId}/convert-to-pdf";

    public static string GetExtractPath(string caseUrn, int caseId, string materialId, long documentId) =>
        $"urns/{caseUrn}/cases/{caseId}/documents/{materialId}/versions/{documentId}/extract";

    public static string GetRemoveCaseIndexesPath(string caseUrn, int caseId) => $"urns/{caseUrn}/cases/{caseId}/remove-case-indexes";

    public static string GetSearchPath(string caseUrn, int caseId) => $"urns/{caseUrn}/cases/{caseId}/search";

    public static string GetRedactPdfPath(string caseUrn, int caseId, string materialId, long documentId) =>
        $"urns/{caseUrn}/cases/{caseId}/documents/{materialId}/versions/{documentId}/redact";

    public static string GetCaseIndexCountResultsPath(string caseUrn, int caseId) => $"urns/{caseUrn}/cases/{caseId}/case-index-count";

    public static string GetDocumentIndexCountResultsPath(string caseUrn, int caseId, string materialId, long documentId) =>
        $"urns/{caseUrn}/cases/{caseId}/documents/{materialId}/versions/{documentId}/document-index-count";

    public static string GetModifyDocumentPath(string caseUrn, int caseId, string materialId, long documentId) =>
        $"urns/{caseUrn}/cases/{caseId}/documents/{materialId}/versions/{documentId}/modify";

    public static string CaseSearchCountPath(string caseUrn, int caseId) => $"urns/{caseUrn}/cases/{caseId}/search/count";

    public static string GetThumbnailPath(string caseUrn, int caseId, string materialId, int documentId, int maxDimensionPixel, int? pageIndex) =>
        $"urns/{caseUrn}/cases/{caseId}/documents/{materialId}/versions/{documentId}/thumbnails/{maxDimensionPixel}/{pageIndex}";

    public static string GetBulkRedactionSearchStartPath(int caseId, string materialId, long documentId) =>
        $"cases/{caseId}/materials/{materialId}/documents/{documentId}/search";

    public static string GetBulkRedactionSearchResultsPath(int caseId, string materialId, long documentId, string searchText) =>
        $"cases/{caseId}/materials/{materialId}/documents/{documentId}/search?SearchText={searchText}";
}