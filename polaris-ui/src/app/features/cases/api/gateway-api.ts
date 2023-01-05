import { ApiError } from "../../../common/errors/ApiError";

import { CaseDocument } from "../domain/CaseDocument";
import { CaseSearchResult } from "../domain/CaseSearchResult";
import { PipelineResults } from "../domain/PipelineResults";
import { ApiTextSearchResult } from "../domain/ApiTextSearchResult";
import { RedactionSaveRequest } from "../domain/RedactionSaveRequest";
import { RedactionSaveResponse } from "../domain/RedactionSaveResponse";
import * as HEADERS from "./header-factory";
import {
  buildFullUrl as buildEncodedUrl,
  fullUrl as buildUnencodedUrl,
} from "./url-helpers";
import { CaseDetails } from "../domain/CaseDetails";
import { CmsDocCategory } from "../domain/CmsDocCategory";

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

export const resolvePdfUrl = (blobNameUrlFragment: string) =>
  buildUnencodedUrl(`/api/pdfs/${blobNameUrlFragment}`);

export const searchUrn = async (urn: string) => {
  const url = buildEncodedUrl({ urn }, ({ urn }) => `/api/urns/${urn}/cases`);
  const headers = await buildHeaders(
    HEADERS.correlationId,
    HEADERS.auth,
    HEADERS.upstreamHeader
  );
  const response = await fetch(url, {
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
  const url = buildEncodedUrl(
    { urn, caseId },
    ({ urn, caseId }) => `/api/urns/${urn}/cases/${caseId}`
  );

  const response = await fetch(url, {
    headers: await buildHeaders(
      HEADERS.correlationId,
      HEADERS.auth,
      HEADERS.upstreamHeader
    ),
  });

  if (!response.ok) {
    throw new ApiError("Get Case Details failed", url, response);
  }

  return (await response.json()) as CaseDetails;
};

export const getCaseDocumentsList = async (urn: string, caseId: number) => {
  const url = buildEncodedUrl(
    { urn, caseId },
    ({ urn, caseId }) => `/api/urns/${urn}/cases/${caseId}/documents`
  );
  const response = await fetch(url, {
    headers: await buildHeaders(
      HEADERS.correlationId,
      HEADERS.auth,
      HEADERS.upstreamHeader
    ),
  });

  if (!response.ok) {
    throw new ApiError("Get Case Documents failed", url, response);
  }

  const apiReponse: CaseDocument[] = await response.json();

  return apiReponse;
};

export const getPdfSasUrl = async (pdfBlobName: string) => {
  const url = buildUnencodedUrl(`/api/pdf/sasUrl/${pdfBlobName}`);
  const response = await fetch(url, {
    headers: await buildHeaders(
      HEADERS.correlationId,
      HEADERS.auth,
      HEADERS.upstreamHeader
    ),
  });

  if (!response.ok) {
    throw new ApiError("Get Pdf SasUrl failed", url, response);
  }

  return await response.text();
};

export const initiatePipeline = async (urn: string, caseId: number) => {
  const path = buildEncodedUrl(
    { urn, caseId },
    ({ urn, caseId }) => `/api/urns/${urn}/cases/${caseId}?force=true`
  );

  const correlationIdHeader = HEADERS.correlationId();
  const response = await fetch(path, {
    headers: await buildHeaders(
      correlationIdHeader,
      HEADERS.auth,
      HEADERS.upstreamHeader
    ),
    method: "POST",
  });

  if (!response.ok) {
    throw new ApiError("Initiate pipeline failed", path, response);
  }

  const { trackerUrl }: { trackerUrl: string } = await response.json();

  return { trackerUrl, correlationId: Object.values(correlationIdHeader)[0] };
};

export const getPipelinePdfResults = async (
  trackerUrl: string,
  existingCorrelationId: string
) => {
  const headers = await buildHeaders(
    HEADERS.correlationId(existingCorrelationId),
    HEADERS.auth,
    HEADERS.upstreamHeader
  );

  const response = await fetch(trackerUrl, {
    headers,
  });

  return (await response.json()) as PipelineResults;
};

export const searchCase = async (
  urn: string,
  caseId: number,
  searchTerm: string
) => {
  const path = buildEncodedUrl(
    { caseId, searchTerm, urn },
    ({ caseId, searchTerm, urn }) =>
      `/api/urns/${urn}/cases/${caseId}/query/${searchTerm}`
  );
  const response = await fetch(path, {
    headers: await buildHeaders(
      HEADERS.correlationId,
      HEADERS.auth,
      HEADERS.upstreamHeader
    ),
  });

  if (!response.ok) {
    throw new ApiError("Search Case Text failed", path, response);
  }

  return (await response.json()) as ApiTextSearchResult[];
};

export const checkoutDocument = async (
  urn: string,
  caseId: number,
  cmsDocCategory: CmsDocCategory,
  docId: number
) => {
  const url = buildEncodedUrl(
    { caseId, docId, cmsDocCategory },
    ({ caseId, docId, cmsDocCategory }) =>
      `/api/urns/${urn}/cases/${caseId}/documents/${cmsDocCategory}/${docId}/checkout`
  );

  const response = await fetch(url, {
    headers: await buildHeaders(
      HEADERS.correlationId,
      HEADERS.auth,
      HEADERS.upstreamHeader
    ),
    method: "POST",
  });

  if (!response.ok) {
    throw new ApiError("Checkout document failed", url, response);
  }

  return true; // unhappy path not known yet
};

export const cancelCheckoutDocument = async (
  urn: string,
  caseId: number,
  cmsDocCategory: CmsDocCategory,
  docId: number
) => {
  const url = buildEncodedUrl(
    { caseId, docId, cmsDocCategory },
    ({ caseId, docId, cmsDocCategory }) =>
      `/api/urns/${urn}/cases/${caseId}/documents/${cmsDocCategory}/${docId}/checkout`
  );

  const response = await fetch(url, {
    headers: await buildHeaders(
      HEADERS.correlationId,
      HEADERS.auth,
      HEADERS.upstreamHeader
    ),
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
  cmsDocCategory: CmsDocCategory,
  docId: number,
  fileName: string,
  redactionSaveRequest: RedactionSaveRequest
) => {
  const url = buildEncodedUrl(
    { urn, caseId, docId, fileName, cmsDocCategory },
    ({ urn, caseId, docId, cmsDocCategory }) =>
      `/api/urns/${urn}/cases/${caseId}/documents/${cmsDocCategory}/${docId}/${fileName}`
  );

  const response = await fetch(url, {
    headers: await buildHeaders(
      HEADERS.correlationId,
      HEADERS.auth,
      HEADERS.upstreamHeader
    ),
    method: "PUT",
    body: JSON.stringify(redactionSaveRequest),
  });

  if (!response.ok) {
    throw new ApiError("Save redactions failed", url, response);
  }

  return (await response.json()) as RedactionSaveResponse;
};
