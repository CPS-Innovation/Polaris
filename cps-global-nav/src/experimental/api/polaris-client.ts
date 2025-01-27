import { buildHeaders } from "../ad-auth/header-factory";
import { GATEWAY_BASE_URL } from "../config";
import { ApiError } from "../../errors/ApiError";
import { UrnLookupResult } from "../domain/UrnLookupResult";
import { fetchWithInSituReauth } from "./auth/fetch-with-in-situ-reauth";
import { CaseDetails } from "../domain/CaseDetails";

const fullUrl = (path: string, baseUrl: string = GATEWAY_BASE_URL) => {
  const origin = baseUrl?.startsWith("http") ? baseUrl : window.location.origin;
  return new URL(path, origin).toString();
};

export const lookupUrn = async (caseId: number) => {
  const url = fullUrl(`/api/urn-lookup/${caseId}`);

  const response = await fetchWithInSituReauth(url, {
    headers: await buildHeaders(),
  });

  if (!response.ok) {
    throw new ApiError(`lookupUrn failed: ${response.status}`);
  }

  return (await response.json()) as UrnLookupResult;
};

export const getCaseDetails = async (urn: string, caseId: number) => {
  const url = fullUrl(`/api/urns/${urn}/cases/${caseId}`);

  const response = await fetchWithInSituReauth(url, {
    headers: await buildHeaders(),
  });

  if (!response.ok) {
    throw new ApiError(`getCaseDetails failed: ${response.status}`);
  }

  return (await response.json()) as CaseDetails;
};
