export const CASE_SEARCH_ROUTE = "api/urns/:urn/cases";
export const CASE_ROUTE = "api/urns/:urn/cases/:caseId";
export const INITIATE_PIPELINE_ROUTE = "api/urns/:urn/cases/:caseId";
export const TRACKER_ROUTE = "api/urns/:urn/cases/:caseId/tracker";
export const FILE_ROUTE = "api/urns/:urn/cases/:caseId/documents/:documentId";
export const GET_SAS_URL_ROUTE =
  "api/urns/:urn/cases/:caseId/documents/:documentId/sasUrl";
export const TEXT_SEARCH_ROUTE =
  "api/urns/:urn/cases/:caseId/documents/search?query=";
export const DOCUMENT_CHECKOUT_ROUTE =
  "api/documents/checkout/:caseId/:documentId";
export const DOCUMENT_CHECKIN_ROUTE =
  "api/documents/checkin/:caseId/:documentId";

export const SAS_URL_ROUTE = "api/some-complicated-sas-url/:blobName";
