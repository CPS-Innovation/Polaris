import { ApiError } from "../../../common/errors/ApiError";
import { CaseSearchResult } from "../domain/gateway/CaseSearchResult";
import { PipelineResults } from "../domain/gateway/PipelineResults";
import { ApiTextSearchResult } from "../domain/gateway/ApiTextSearchResult";
import { RedactionSaveRequest } from "../domain/gateway/RedactionSaveRequest";
import * as HEADERS from "./header-factory";
import { CaseDetails } from "../domain/gateway/CaseDetails";
import { reauthenticationFilter } from "./reauthentication-filter";
import { GATEWAY_BASE_URL } from "../../../config";
import { LOCKED_STATUS_CODE } from "../hooks/utils/refreshUtils";

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

const fullUrl = (path: string) => {
  const origin = GATEWAY_BASE_URL?.startsWith("http")
    ? GATEWAY_BASE_URL
    : window.location.origin;
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

export const searchUrn = async (urn: string) => {
  const url = fullUrl(`/api/urns/${urn}/cases`);
  const headers = await buildHeaders(HEADERS.correlationId, HEADERS.auth);
  const response = await internalReauthenticatingFetch(url, {
    headers,
  });

  if (!response.ok) {
    // special case: the gateway returns 404 if no results
    //  but we are happy to just return empty data
    if (response.status === 404) {
      return [];
    }
    throw new ApiError("Search URN failed", url, response);
  }

  return (await response.json()) as CaseSearchResult[];
};

export const getCaseDetails = async (urn: string, caseId: number) => {
  const url = fullUrl(`/api/urns/${urn}/cases/${caseId}`);

  const response = await internalReauthenticatingFetch(url, {
    headers: await buildHeaders(HEADERS.correlationId, HEADERS.auth),
  });

  if (!response.ok) {
    throw new ApiError("Get Case Details failed", url, response);
  }

  return (await response.json()) as CaseDetails;
};

export const getPdfSasUrl = async (
  urn: string,
  caseId: number,
  documentId: string
) => {
  const url = fullUrl(
    `api/urns/${urn}/cases/${caseId}/documents/${documentId}/sas-url`
  );
  const response = await internalFetch(url, {
    headers: await buildHeaders(HEADERS.correlationId, HEADERS.auth),
  });

  if (!response.ok) {
    throw new ApiError("Get Pdf SasUrl failed", url, response);
  }

  return await response.text();
};

export const initiatePipeline = async (
  urn: string,
  caseId: number,
  correlationId: string
) => {
  const path = fullUrl(`/api/urns/${urn}/cases/${caseId}`);

  const correlationIdHeader = HEADERS.correlationId(correlationId);
  const response = await internalFetch(path, {
    headers: await buildHeaders(correlationIdHeader, HEADERS.auth),
    method: "POST",
  });

  if (!response.ok && response.status !== LOCKED_STATUS_CODE) {
    throw new ApiError("Initiate pipeline failed", path, response);
  }

  const { trackerUrl }: { trackerUrl: string } = await response.json();

  return {
    trackerUrl,
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

  const response = await internalFetch(trackerUrl, {
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
  const response = await internalFetch(path, {
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

  const response = await internalFetch(url, {
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

  const response = await internalFetch(url, {
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

  const response = await internalFetch(url, {
    headers: await buildHeaders(HEADERS.correlationId, HEADERS.auth),
    method: "PUT",
    body: JSON.stringify(redactionSaveRequest),
  });

  if (!response.ok) {
    throw new ApiError("Save redactions failed", url, response);
  }
};

const internalFetch = async (...args: Parameters<typeof fetch>) => {
  return await fetch(args[0], {
    ...args[1],
    // We need cookies to be sent to the gateway, which is a third-party domain,
    //  so need to set `credentials: "include"`
    credentials: "include",
  });
};

const internalReauthenticatingFetch = async (
  ...args: Parameters<typeof fetch>
) => {
  const response = await internalFetch(...args);

  return reauthenticationFilter(response, window);
};
