export const URN_LOOKUP_ROUTE = "api/urn-lookup/:caseId";
export const CASE_SEARCH_ROUTE = "api/urns/:urn/cases";
export const CASE_ROUTE = "api/urns/:urn/cases/:caseId";
export const INITIATE_PIPELINE_ROUTE = "api/urns/:urn/cases/:caseId";
export const TRACKER_ROUTE = "api/urns/:urn/cases/:caseId/tracker";
export const FILE_ROUTE =
  "api/urns/:urn/cases/:caseId/documents/:documentId/versions/:versionId/pdf";
export const TEXT_SEARCH_ROUTE = "api/urns/:urn/cases/:caseId/search?query=";
export const DOCUMENT_CHECKOUT_ROUTE =
  "/api/urns/:urn/cases/:caseId/documents/:documentId/versions/:versionId/checkout";
export const DOCUMENT_CHECKIN_ROUTE =
  "/api/urns/:urn/cases/:caseId/documents/:documentId/versions/:versionId/checkout";
export const SAVE_REDACTION_ROUTE =
  "api/urns/:urn/cases/:caseId/documents/:documentId/versions/:versionId/redact";

export const REDACTION_LOG_LOOKUP_ROUTE = "/api/lookUps";
export const REDACTION_LOG_MAPPING_ROUTE = "/api/polarisMappings";

export const SAVE_REDACTION_LOG_ROUTE = "/api/redactionLogs";

export const NOTES_ROUTE =
  "api/urns/:urn/cases/:caseId/documents/:documentId/notes";

export const SEARCH_PII_ROUTE =
  "api/urns/:urn/cases/:caseId/documents/:documentId/versions/:versionId/pii";

export const RENAME_DOCUMENT_ROUTE =
  "api/urns/:urn/cases/:caseId/documents/:documentId/rename";

export const MATERIAL_TYPE_LIST = "api/reference/reclassification";

export const EXHIBIT_PRODUCERS =
  "api/urns/:urn/cases/:caseId/exhibit-producers";

export const STATEMENT_WITNESS = "api/urns/:urn/cases/:caseId/witnesses";

export const STATEMENT_WITNESS_NUMBERS =
  "api/urns/:urn/cases/:caseId/witnesses/:witnessId/statements";

export const SAVE_RECLASSIFY =
  "/api/urns/:urn/cases/:caseId/documents/:docId/reclassify";

export const SAVE_ROTATION_ROUTE =
  "api/urns/:urn/cases/:caseId/documents/:documentId/versions/:versionId/modify";

export const GET_DOCUMENTS_LIST_ROUTE = "api/urns/:urn/cases/:caseId/documents";

export const TOGGLE_USED_DOCUMENT_STATE_ROUTE = "api/urns/:urn";