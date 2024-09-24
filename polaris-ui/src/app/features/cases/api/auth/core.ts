import { CmsAuthError } from "../../../../common/errors/CmsAuthError";
import {
  REAUTH_REDIRECT_URL_OUTBOUND,
  REAUTH_REDIRECT_URL_OUTBOUND_E2E,
  REAUTH_REDIRECT_URL_INBOUND,
  REAUTH_USE_IN_SITU_REFRESH,
} from "../../../../config";

export const REAUTHENTICATION_INDICATOR_QUERY_PARAM = "auth-refresh";
export const FAIL_CORRELATION_ID_QUERY_PARAM = "fail-correlation-id";
export const AUTH_FAIL_REASON_QUERY_PARAM = "auth-fail-reason";
export const CORRELATION_ID = "Correlation-Id";

export const PREFERRED_AUTH_MODE = REAUTH_USE_IN_SITU_REFRESH
  ? "in-situ"
  : ("full-window" as const);

export const STATUS_CODES = {
  UNAUTHORIZED: 401 as const,
  FORBIDDEN_STATUS_CODE: 403 as const,
  GONE_STATUS_CODE: 410 as const,
  UNAVAILABLE_FOR_LEGAL_REASONS_STATUS_CODE: 451 as const,
};

export enum AuthFailReason {
  NoCookies = "no-cookies",
  NoCmsAuthCookie = "no-cmsauth-cookie",
  CmsAuthNotValid = "cms-auth-not-valid",
  CmsModernAuthNotValid = "cms-modern-auth-not-valid",
  InSituAttemptFailed = "in-situ-attempt-failed",
  UnexpectedError = "unexpected-error",
}

export const buildCmsAuthError = (
  authFailReason: AuthFailReason | null | undefined
) => {
  let customMessage: string;
  switch (authFailReason) {
    case AuthFailReason.NoCookies:
    case AuthFailReason.NoCmsAuthCookie:
      customMessage =
        "It may be the case that you are not yet logged in to CMS, or that the CMS tab that you have been using has recently logged you out.";
      break;
    case AuthFailReason.CmsAuthNotValid:
    case AuthFailReason.CmsModernAuthNotValid:
      customMessage =
        "It may be the case that your CMS session has expired, or you may have logged in to CMS on another computer since logging in to this one.";
      break;
    case AuthFailReason.InSituAttemptFailed:
      customMessage =
        "The attempt made by Casework App to reconnect to your CMS session did not succeed.";
      break;
    case AuthFailReason.UnexpectedError:
    default:
      customMessage =
        "An unexpected error occurred related to your current CMS log in session.";
  }

  return new CmsAuthError(
    `Unable to connect to CMS (${authFailReason}).`,
    customMessage || undefined
  );
};

/*

export const buildRedirectUrl = (
  terminationUrl: string,
  correlationId: string | null | undefined
) => {
  const delimiter = window.location.href.includes("?") ? "&" : "?";
  let nextUrl = `${encodeURIComponent(
    terminationUrl + delimiter + REAUTHENTICATION_INDICATOR_QUERY_PARAM
  )}`;

  if (correlationId) {
    nextUrl += `&${FAIL_CORRELATION_ID_QUERY_PARAM}=${correlationId}`;
  }

  return `${REAUTH_REDIRECT_URL}${encodeURIComponent(nextUrl)}`;
};

*/

export const buildRedirectUrl = (
  window: Window,
  correlationId: string | null
) => {
  // We are going to redirect to an endpoint that will try to re-establish auth
  //  and then redirect back to the current UI URL address.
  return (
    // First we need the URL to redirect to. Typically this will be a relative URL
    //  e.g. /foo but in local development can be a full URL e.g. http://localhost/foo.
    // Special case: the e2e tests run either locally on dev machines or on Azure DeOps
    //  build agents - neither of these environments are able to see the live
    //  handover URLS (which run on e.g. cin3.cps.gov.uk or cms.cps.gov.uk).  So if we
    //  know we are in a cypress test let's redirect via our own controlled endpoint.
    ((window.Cypress && REAUTH_REDIRECT_URL_OUTBOUND_E2E) ||
      // If there is an empty REAUTH_REDIRECT_URL_OUTBOUND_E2E setting or we are not in an e2e test then
      //  obviously we fall back to the live handover endpoint for the environment.
      REAUTH_REDIRECT_URL_OUTBOUND) +
    // The reauth logic running on the destination URL will be expecting a ?r= query
    //  param containing a further URL to redirect on to.
    "?r=" +
    // The parameter is going to contain URLs so will need encoding.
    encodeURIComponent(
      // In the CWA reauth flow this URL is typically also a relative URL.  This next endpoint
      //  is responsible for setting the auth cookies on our domain. It will send a response
      //  with Set-Cookie headers and will do so in a redirect response.
      REAUTH_REDIRECT_URL_INBOUND +
        //This location will want to redirect onwards to a URL it expects to find in a polaris-ui-url param.
        "?polaris-ui-url=" +
        // Another URL so more encoding required
        encodeURIComponent(
          // This final URL will be our current UI url...
          window.location.href +
            // ... with an additional query param attached so the UI knows that it is loading up as
            //  a result of a reauth flow having happened).
            (window.location.href.includes("?") ? "&" : "?") +
            REAUTHENTICATION_INDICATOR_QUERY_PARAM
        ) +
        // The reauth feature is interested in the correlation id of the original API call that suffered
        //  the auth fail.  We append it if it is known.
        (correlationId ? `&fail-correlation-id=${correlationId}` : "")
      // finally close the outer encodeURIComponent and we are done
    )
  );
};

export const isCmsAuthFail = (response: Response) =>
  response.status === STATUS_CODES.UNAUTHORIZED;

export const getCorrelationIdFromFetchArgs = (
  ...args: Parameters<typeof fetch>
) => {
  const headers = args[1]?.headers as Record<string, string> | undefined;
  return headers?.[CORRELATION_ID];
};
