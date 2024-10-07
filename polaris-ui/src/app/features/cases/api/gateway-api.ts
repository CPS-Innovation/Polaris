import { ApiError } from "../../../common/errors/ApiError";
import { CaseSearchResult } from "../domain/gateway/CaseSearchResult";
import { PipelineResults } from "../domain/gateway/PipelineResults";
import { ApiTextSearchResult } from "../domain/gateway/ApiTextSearchResult";
import { RedactionSaveRequest } from "../domain/gateway/RedactionSaveRequest";
import * as HEADERS from "./auth/header-factory";
import { CaseDetails } from "../domain/gateway/CaseDetails";
import { GATEWAY_BASE_URL, REDACTION_LOG_BASE_URL } from "../../../config";
import { LOCKED_STATUS_CODE } from "../hooks/utils/refreshUtils";
import {
  RedactionLogLookUpsData,
  RedactionLogMappingData,
} from "../domain/redactionLog/RedactionLogData";
import { RedactionLogRequestData } from "../domain/redactionLog/RedactionLogRequestData";
import { Note } from "../domain/gateway/NotesData";
import { SearchPIIResultItem } from "../domain/gateway/SearchPIIData";
import { removeNonDigits } from "../presentation/case-details/utils/redactionLogUtils";
import { MaterialType } from "../presentation/case-details/reclassify/data/MaterialType";
import { ExhibitProducer } from "../presentation/case-details/reclassify/data/ExhibitProducer";
import { StatementWitness } from "../presentation/case-details/reclassify/data/StatementWitness";
import { StatementWitnessNumber } from "../presentation/case-details/reclassify/data/StatementWitnessNumber";
import { ReclassifySaveData } from "../presentation/case-details/reclassify/data/ReclassifySaveData";
import { UrnLookupResult } from "../domain/gateway/UrnLookupResult";
import { fetchWithFullWindowReauth } from "./auth/fetch-with-full-window-reauth";
import {
  fetchWithInSituReauth,
  fetchWithProactiveInSituReauth,
} from "./auth/fetch-with-in-situ-reauth";
import { fetchWithCookies } from "./auth/fetch-with-cookies";
import { FetchArgs, PREFERRED_AUTH_MODE, STATUS_CODES } from "./auth/core";

const buildHeaders = async (
  ...args: (
    | Record<string, string>
    | (() => Record<string, string>)
    | (() => Promise<Record<string, string>>)
  )[]
) => {
  let headers = {} as Record<string, string>;
  for (const arg of args) {
    const header = typeof arg === "function" ? await arg() : arg; // unwrap if a promise. otherwise all good
    headers = { ...headers, ...header };
  }

  return headers;
};

const fullUrl = (path: string, baseUrl: string = GATEWAY_BASE_URL) => {
  const origin = baseUrl?.startsWith("http") ? baseUrl : window.location.origin;
  return new URL(path, origin).toString();
};

// hack
const temporaryApiModelMapping = (arr: any[]) =>
  arr.forEach((item) => {
    if (item.polarisDocumentId) {
      item.documentId = item.polarisDocumentId;
      if (item.cmsDocType?.documentTypeId) {
        item.cmsDocType.documentTypeId = parseInt(
          item.cmsDocType.documentTypeId,
          10
        );
      }
    }
  });

export const resolvePdfUrl = (
  urn: string,
  caseId: number,
  documentId: string,
  polarisDocumentVersionId: number
) => {
  return fullUrl(
    `api/urns/${urn}/cases/${caseId}/documents/${documentId}?v=${polarisDocumentVersionId}`
  );
};

export const lookupUrn = async (caseId: number) => {
  const url = fullUrl(`/api/urn-lookup/${caseId}`);
  const headers = await buildHeaders(HEADERS.correlationId, HEADERS.auth);
  const response = await reauthenticatingFetch(url, {
    headers,
  });

  if (!response.ok) {
    throw caseCallErrorFactory(response, url, "Lookup URN failed");
  }

  return (await response.json()) as UrnLookupResult;
};

export const searchUrn = async (urn: string) => {
  const url = fullUrl(`/api/urns/${urn}/cases`);
  const headers = await buildHeaders(HEADERS.correlationId, HEADERS.auth);
  const response = await reauthenticatingFetch(url, {
    headers,
  });

  if (!response.ok) {
    throw caseCallErrorFactory(response, url, "Search URN failed");
  }

  return (await response.json()) as CaseSearchResult[];
};

export const getCaseDetails = async (urn: string, caseId: number) => {
  const url = fullUrl(`/api/urns/${urn}/cases/${caseId}`);

  const response = await reauthenticatingFetch(url, {
    headers: await buildHeaders(HEADERS.correlationId, HEADERS.auth),
  });

  if (!response.ok) {
    throw caseCallErrorFactory(response, url, "Get Case Details failed");
  }

  return (await response.json()) as CaseDetails;
};

export const initiatePipeline = async (
  urn: string,
  caseId: number,
  correlationId: string
) => {
  const path = fullUrl(`/api/urns/${urn}/cases/${caseId}`);

  const correlationIdHeader = HEADERS.correlationId(correlationId);
  const response = await reauthenticatingFetch(path, {
    headers: await buildHeaders(correlationIdHeader, HEADERS.auth),
    method: "POST",
  });

  if (!response.ok && response.status !== LOCKED_STATUS_CODE) {
    throw new ApiError("Initiate pipeline failed", path, response);
  }

  const { trackerUrl }: { trackerUrl: string } = await response.json();

  return {
    trackerUrl: fullUrl(trackerUrl),
    correlationId: Object.values(correlationIdHeader)[0],
    status: response.status,
  };
};

export const getPipelinePdfResults = async (
  trackerUrl: string,
  existingCorrelationId: string
): Promise<false | PipelineResults> => {
  const headers = await buildHeaders(
    HEADERS.correlationId(existingCorrelationId),
    HEADERS.auth
  );

  const response = await reauthenticatingFetch(trackerUrl, {
    headers,
  });
  // we are ignoring the tracker status 404 as it is an expected one and continue polling
  if (response.status === 404) {
    return false;
  }
  if (!response.ok) {
    throw new ApiError("Get Pipeline pdf results failed", trackerUrl, response);
  }
  const rawResponse: { documents: any[] } = await response.json();
  const { documents } = rawResponse;
  temporaryApiModelMapping(documents);

  // temporary hack for #24313 before feature flag comes in
  // return rawResponse as PipelineResults;
  var typedRawResponse = rawResponse as PipelineResults;

  return typedRawResponse;
};
export const searchCase = async (
  urn: string,
  caseId: number,
  searchTerm: string
) => {
  const path = fullUrl(
    `/api/urns/${urn}/cases/${caseId}/search/?query=${searchTerm}`
  );
  const response = await reauthenticatingFetch(path, {
    headers: await buildHeaders(HEADERS.correlationId, HEADERS.auth),
  });

  if (!response.ok) {
    throw new ApiError("Search Case Text failed", path, response);
  }

  const rawResponse = await response.json();
  temporaryApiModelMapping(rawResponse);

  return rawResponse as ApiTextSearchResult[];
};

export const checkoutDocument = async (
  urn: string,
  caseId: number,
  documentId: string
) => {
  const url = fullUrl(
    `/api/urns/${urn}/cases/${caseId}/documents/${documentId}/checkout`
  );

  const response = await reauthenticatingFetch(url, {
    headers: await buildHeaders(HEADERS.correlationId, HEADERS.auth),
    method: "POST",
  });

  if (!response.ok) {
    throw new ApiError("Checkout document failed", url, response, {
      username: await response.text(),
    });
  }

  return true; // unhappy path not known yet
};

export const cancelCheckoutDocument = async (
  urn: string,
  caseId: number,
  documentId: string
) => {
  const url = fullUrl(
    `/api/urns/${urn}/cases/${caseId}/documents/${documentId}/checkout`
  );

  const response = await reauthenticatingFetch(url, {
    headers: await buildHeaders(HEADERS.correlationId, HEADERS.auth),
    method: "DELETE",
  });

  if (!response.ok) {
    throw new ApiError("Checkin document failed", url, response);
  }

  return true; // unhappy path not known yet
};

export const saveRedactions = async (
  urn: string,
  caseId: number,
  documentId: string,
  redactionSaveRequest: RedactionSaveRequest
) => {
  const url = fullUrl(
    `/api/urns/${urn}/cases/${caseId}/documents/${documentId}`
  );

  const response = await proactiveReauthenticatingFetch(url, {
    headers: await buildHeaders(HEADERS.correlationId, HEADERS.auth),
    method: "PUT",
    body: JSON.stringify(redactionSaveRequest),
  });

  if (!response.ok) {
    throw new ApiError("Save redactions failed", url, response);
  }
};

export const saveRedactionLog = async (
  redactionLogRequestData: RedactionLogRequestData
) => {
  const url = fullUrl(`/api/redactionLogs`, REDACTION_LOG_BASE_URL);
  const response = await reauthenticatingFetch(url, {
    headers: await buildHeaders(
      HEADERS.correlationId,
      HEADERS.authRedactionLog
    ),
    method: "POST",
    body: JSON.stringify(redactionLogRequestData),
  });

  if (!response.ok) {
    throw new ApiError("Save redaction log failed", url, response);
  }
};

export const getRedactionLogLookUpsData = async () => {
  const url = fullUrl("/api/lookUps", REDACTION_LOG_BASE_URL);
  const headers = await buildHeaders(
    HEADERS.correlationId,
    HEADERS.authRedactionLog
  );
  const response = await nonReauthenticatingFetch(url, {
    headers,
  });
  if (!response.ok) {
    throw new ApiError("Get Redaction Log data failed", url, response);
  }
  return (await response.json()) as RedactionLogLookUpsData;
};

export const getRedactionLogMappingData = async () => {
  const url = fullUrl("/api/polarisMappings", REDACTION_LOG_BASE_URL);
  const headers = await buildHeaders(
    HEADERS.correlationId,
    HEADERS.authRedactionLog
  );
  const response = await nonReauthenticatingFetch(url, {
    headers,
  });

  if (!response.ok) {
    throw new ApiError("Get Redaction Log mapping data failed", url, response);
  }
  return (await response.json()) as RedactionLogMappingData;
};

export const getNotesData = async (
  urn: string,
  caseId: number,
  documentId: string
) => {
  const docId = parseInt(removeNonDigits(documentId));
  const path = fullUrl(
    `/api/urns/${urn}/cases/${caseId}/documents/${docId}/notes`
  );

  const response = await reauthenticatingFetch(path, {
    headers: await buildHeaders(HEADERS.correlationId, HEADERS.auth),
  });

  if (!response.ok) {
    throw new ApiError("Get Notes failed", path, response);
  }

  return (await response.json()) as Note[];
};

export const addNoteData = async (
  urn: string,
  caseId: number,
  documentId: string,
  text: string
) => {
  const docId = parseInt(removeNonDigits(documentId));
  const path = fullUrl(
    `/api/urns/${urn}/cases/${caseId}/documents/${docId}/notes`
  );

  const response = await reauthenticatingFetch(path, {
    headers: await buildHeaders(HEADERS.correlationId, HEADERS.auth),
    method: "POST",
    body: JSON.stringify({ documentId: docId, text: text }),
  });

  if (!response.ok) {
    throw new ApiError("Add Notes failed", path, response);
  }

  return true;
};

export const saveDocumentRename = async (
  urn: string,
  caseId: number,
  documentId: string,
  name: string
) => {
  const docId = parseInt(removeNonDigits(documentId));
  const path = fullUrl(
    `/api/urns/${urn}/cases/${caseId}/documents/${docId}/rename`
  );

  const response = await reauthenticatingFetch(path, {
    headers: await buildHeaders(HEADERS.correlationId, HEADERS.auth),
    method: "PUT",
    body: JSON.stringify({ documentId: docId, documentName: name }),
  });

  if (!response.ok) {
    throw new ApiError("Rename document failed", path, response);
  }

  return true;
};

export const getSearchPIIData = async (
  urn: string,
  caseId: number,
  documentId: string
) => {
  const path = fullUrl(
    `/api/urns/${urn}/cases/${caseId}/documents/${documentId}/pii`
  );

  const response = await reauthenticatingFetch(path, {
    headers: await buildHeaders(HEADERS.correlationId, HEADERS.auth),
  });

  if (!response.ok) {
    throw new ApiError("Get search PII data failed", path, response);
  }

  return (await response.json()) as SearchPIIResultItem[];
};

export const getMaterialTypeList = async () => {
  const path = fullUrl(`/api/reference/reclassification`);

  const response = await nonReauthenticatingFetch(path, {
    headers: await buildHeaders(HEADERS.correlationId, HEADERS.auth),
  });

  if (!response.ok) {
    throw new ApiError("Get material type list failed", path, response);
  }

  return (await response.json()) as MaterialType[];
};

export const getExhibitProducers = async (urn: string, caseId: number) => {
  const path = fullUrl(`/api/urns/${urn}/cases/${caseId}/exhibit-producers`);

  const response = await reauthenticatingFetch(path, {
    headers: await buildHeaders(HEADERS.correlationId, HEADERS.auth),
  });

  if (!response.ok) {
    throw new ApiError("Get exhibit details failed", path, response);
  }

  return (await response.json()) as ExhibitProducer[];
};

export const getStatementWitnessDetails = async (
  urn: string,
  caseId: number
) => {
  const path = fullUrl(`/api/urns/${urn}/cases/${caseId}/witnesses`);

  const response = await reauthenticatingFetch(path, {
    headers: await buildHeaders(HEADERS.correlationId, HEADERS.auth),
  });

  if (!response.ok) {
    throw new ApiError("Get statement details failed", path, response);
  }

  return (await response.json()) as StatementWitness[];
};

export const getWitnessStatementNumbers = async (
  urn: string,
  caseId: number,
  witnessId: number
) => {
  const path = fullUrl(
    `/api/urns/${urn}/cases/${caseId}/witnesses/${witnessId}/statements`
  );

  const response = await reauthenticatingFetch(path, {
    headers: await buildHeaders(HEADERS.correlationId, HEADERS.auth),
  });

  if (!response.ok) {
    throw new ApiError("Get witness statement numbers failed", path, response);
  }

  return (await response.json()) as StatementWitnessNumber[];
};

export const saveDocumentReclassify = async (
  urn: string,
  caseId: number,
  documentId: string,
  data: ReclassifySaveData
) => {
  const docId = parseInt(removeNonDigits(documentId));
  const path = fullUrl(
    `/api/urns/${urn}/cases/${caseId}/documents/${docId}/reclassify`
  );

  const response = await reauthenticatingFetch(path, {
    headers: await buildHeaders(HEADERS.correlationId, HEADERS.auth),
    method: "POST",
    body: JSON.stringify(data),
  });

  return response.ok;
};

const nonReauthenticatingFetch = async (...args: FetchArgs) =>
  fetchWithCookies(...args);

const reauthenticatingFetch = async (...args: FetchArgs) =>
  PREFERRED_AUTH_MODE === "in-situ"
    ? fetchWithInSituReauth(...args)
    : fetchWithFullWindowReauth(...args);

const proactiveReauthenticatingFetch = async (...args: FetchArgs) =>
  PREFERRED_AUTH_MODE === "in-situ"
    ? fetchWithProactiveInSituReauth(...args)
    : // there is not a proactive equivalent (yet, or ever?) in the full-page reauth flow.
      fetchWithFullWindowReauth(...args);

const caseCallErrorFactory = (
  response: Response,
  url: string,
  errorMessage: string
) => {
  const knownMessageMap: { [key: number]: [string, string] } = {
    [STATUS_CODES.FORBIDDEN_STATUS_CODE]: [
      "You do not have access to this case.",
      "You do not have access to this case.",
    ],
    [STATUS_CODES.GONE_STATUS_CODE]: [
      "This case no longer exists.",
      "This case no longer exists.",
    ],
    [STATUS_CODES.UNAVAILABLE_FOR_LEGAL_REASONS_STATUS_CODE]: [
      "CMS Modern unauthorized.",
      "It looks like you do not have access to CMS Modern, please contact the service desk.",
    ],
  };

  const knownMessage = knownMessageMap[response.status];

  return knownMessage
    ? new ApiError(knownMessage[0], url, response, undefined, knownMessage[1])
    : new ApiError(errorMessage, url, response);
};
