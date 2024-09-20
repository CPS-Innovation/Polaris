import { CmsAuthError } from "../../../common/errors/CmsAuthError";
import { CmsAuthRedirectingError } from "../../../common/errors/CmsAuthRedirectingError";
import {
  REAUTH_REDIRECT_URL_OUTBOUND,
  REAUTH_REDIRECT_URL_OUTBOUND_E2E,
  REAUTH_REDIRECT_URL_INBOUND,
} from "../../../config";

const REAUTHENTICATION_INDICATOR_QUERY_PARAM = "auth-refresh";

enum AuthFailReason {
  NoCookies = "no-cookies",
  NoCmsAuthCookie = "no-cmsauth-cookie",
  CmsAuthNotValid = "cms-auth-not-valid",
  CmsModernAuthNotValid = "cms-modern-auth-not-valid",
  UnexpectedError = "unexpected-error",
}

const getQueryParam = (window: Window, param: string): string | null => {
  const urlParams = new URLSearchParams(window.location.search);
  return urlParams.get(param);
};

const isCmsAuthFail = (response: Response) => response.status === 401;

const isAuthPageLoad = (window: Window) =>
  window.location.href.includes(REAUTHENTICATION_INDICATOR_QUERY_PARAM);

const buildRedirectUrl = (window: Window, correlationId: string | null) => {
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

const tryCleanRefreshIndicator = (window: Window) => {
  // clean the indicator from the browser address bar
  if (window.location.href.includes(REAUTHENTICATION_INDICATOR_QUERY_PARAM)) {
    const nextUrl = window.location.href.replace(
      new RegExp(`[?|&]${REAUTHENTICATION_INDICATOR_QUERY_PARAM}`),
      ""
    );

    window.history.replaceState(null, "", nextUrl);
  }
};

const tryHandleFirstAuthFail = (
  response: Response,
  window: Window,
  correlationId: string | null
) => {
  if (isCmsAuthFail(response) && !isAuthPageLoad(window)) {
    window.location.href = buildRedirectUrl(window, correlationId);
    // stop any follow-on logic occurring
    throw new CmsAuthRedirectingError();
  }
  return null;
};

const renderAuthFailReason = (authFailReason: AuthFailReason | null) => {
  switch (authFailReason) {
    case AuthFailReason.NoCookies:
    case AuthFailReason.NoCmsAuthCookie:
      return "It may be the case that you are not yet logged in to CMS, or that CMS has recently logged you out.";
    case AuthFailReason.CmsAuthNotValid:
    case AuthFailReason.CmsModernAuthNotValid:
      return "It may be the case that your CMS session has expired.";
    case AuthFailReason.UnexpectedError:
    default:
      return "An unexpected error occurred related to your current CMS log in session.";
  }
};

const handleSecondAuthFail = (response: Response, window: Window) => {
  if (isCmsAuthFail(response) && isAuthPageLoad(window)) {
    const authFailReasonParam = getQueryParam(
      window,
      "auth-fail-reason"
    ) as AuthFailReason | null;

    const customMessage = renderAuthFailReason(authFailReasonParam);

    throw new CmsAuthError(
      "Unable to connect to CMS.",
      customMessage || undefined
    );
  }
  return null;
};

const handleNonAuthCall = (response: Response, window: Window) => {
  tryCleanRefreshIndicator(window);
  return response;
};

export const reauthenticationFilter = (
  response: Response,
  window: Window,
  correlationId: string | null
) =>
  tryHandleFirstAuthFail(response, window, correlationId) ||
  handleSecondAuthFail(response, window) ||
  handleNonAuthCall(response, window);
