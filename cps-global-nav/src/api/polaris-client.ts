import { buildHeaders } from "../auth/header-factory";
import { GATEWAY_BASE_URL } from "../config";

import { UrnLookupResult } from "../types/UrnLookupResult";
import { fetchWithInSituReauth } from "./auth/fetch-with-in-situ-reauth";

const fullUrl = (path: string, baseUrl: string = GATEWAY_BASE_URL) => {
  const origin = baseUrl?.startsWith("http") ? baseUrl : window.location.origin;
  return new URL(path, origin).toString();
};

export const lookupUrn = async (caseId: number) => {
  const url = fullUrl(`/api/urn-lookup/${caseId}`);
  const headers = await buildHeaders();

  const response = await fetchWithInSituReauth(url, {
    headers,
    credentials: "include",
  });

  if (!response.ok) {
    throw new Error(`Fetch failed: ${response.status}`);
  }

  return (await response.json()) as UrnLookupResult;
};
