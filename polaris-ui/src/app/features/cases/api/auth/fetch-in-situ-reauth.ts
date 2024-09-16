import fetchJsonp from "fetch-jsonp";
import {
  AuthFailReason,
  buildCmsAuthError,
  buildRedirectUrl,
  getCorrelationIdFromFetchArgs,
  isCmsAuthFail,
} from "./core";
import { fetchWithCookies } from "./fetch-with-cookies";

type InSituReauthResult = {
  isSuccess: boolean;
  failReason: AuthFailReason | null | undefined;
};

const inSituReauth = async (
  correlationId: string | undefined
): Promise<InSituReauthResult> => {
  // We cannot quite use the native functionality of `fetch-jsonp` as we
  // should be using the `r=...` parameter for our return URL. So we mimic
  // FWIW by using the same terminology as the library uses and assemble the callback
  // param ourselves.
  const jsonpCallbackFunction = `jsonp_${Date.now()}_${Math.ceil(
    Math.random() * 100000
  )}`;

  const reauthUrl = buildRedirectUrl(
    // EXPERIMENTAL!! todo: we do not want to redirect to /api/auth-refresh-termination
    //  rather we want /auth-refresh-termination.  We need the /api/... variant for local
    //  development, so lets feed the url fragment in from terraform/config.
    `/api/auth-refresh-termination?cb=${jsonpCallbackFunction}`,
    correlationId
  );
  const reauthResult = await fetchJsonp(reauthUrl, { jsonpCallbackFunction });
  return (await reauthResult.json()) as InSituReauthResult;
};

export const fetchInSituReauth = async (...args: Parameters<typeof fetch>) => {
  const response = await fetchWithCookies(...args);
  if (!isCmsAuthFail(response)) {
    return response;
  }

  const correlationId = getCorrelationIdFromFetchArgs(...args);
  const { isSuccess, failReason } = await inSituReauth(correlationId);
  console.log({ isSuccess, failReason });
  if (isSuccess) {
    return fetchWithCookies(...args);
  } else {
    throw buildCmsAuthError(failReason);
  }
};
