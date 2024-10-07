import { CmsAuthRedirectingError } from "../../../../common/errors/CmsAuthRedirectingError";
import {
  REAUTH_REDIRECT_URL_INBOUND,
  REAUTH_REDIRECT_URL_OUTBOUND,
  REAUTH_REDIRECT_URL_OUTBOUND_E2E,
} from "../../../../config";
import {
  assembleRedirectUrl,
  AUTH_FAIL_REASON_QUERY_PARAM,
  AuthFailReason,
  buildCmsAuthError,
  FetchArgs,
  getCorrelationIdFromFetchArgs,
  isCmsAuthFail,
  REAUTHENTICATION_INDICATOR_QUERY_PARAM,
} from "./core";
import { fetchWithCookies } from "./fetch-with-cookies";

const getQueryParam = (window: Window, param: string): string | null => {
  const urlParams = new URLSearchParams(window.location.search);
  return urlParams.get(param);
};

const isAuthPageLoad = (window: Window) =>
  window.location.href.includes(REAUTHENTICATION_INDICATOR_QUERY_PARAM);

const cleanRefreshIndicator = (window: Window) => {
  // clean the indicator from the browser address bar
  if (window.location.href.includes(REAUTHENTICATION_INDICATOR_QUERY_PARAM)) {
    let url = new URL(window.location.href);
    url.searchParams.delete(REAUTHENTICATION_INDICATOR_QUERY_PARAM);
    url.searchParams.delete(AUTH_FAIL_REASON_QUERY_PARAM);
    window.history.replaceState(null, "", url.toString());
  }
};

const buildRedirectUrl = (window: Window, correlationId: string | null) =>
  assembleRedirectUrl({
    outboundUrl:
      (window.Cypress && REAUTH_REDIRECT_URL_OUTBOUND_E2E) ||
      REAUTH_REDIRECT_URL_OUTBOUND,
    inboundUrl: REAUTH_REDIRECT_URL_INBOUND,
    terminationUrl:
      window.location.href +
      (window.location.href.includes("?") ? "&" : "?") +
      REAUTHENTICATION_INDICATOR_QUERY_PARAM,
    correlationId,
  });

const tryHandleAsFirstAuthFail = (
  response: Response,
  window: Window,
  correlationId: string | null
) => {
  if (isCmsAuthFail(response) && !isAuthPageLoad(window)) {
    // If this response is an auth fail, we will be sending the entire browser window
    //  around the re-auth loop by setting `window.location.href` and then throwing an
    //  error to halt our code execution and prevent any follow-on functionality from firing
    //  inadvertently.
    window.location.href = buildRedirectUrl(window, correlationId);

    // stop any follow-on logic occurring
    throw new CmsAuthRedirectingError();
  }
  return null;
};

const tryHandleAsSecondAuthFail = (response: Response, window: Window) => {
  if (isCmsAuthFail(response) && isAuthPageLoad(window)) {
    // We are here as a fetch has failed due to auth, but we know that this is a retry
    //  after the browser has gone around the reauth loop by looking at url for the
    //  `auth-refresh` parameter.
    // Unfortunately this is the end of the road because we have tried to re-establish
    //  auth credentials but the call has failed a second time.  This means that there is
    //  not much more to do apart from throw a CmsAuthError.
    const authFailReasonParam = getQueryParam(
      window,
      AUTH_FAIL_REASON_QUERY_PARAM
    ) as AuthFailReason | null;

    cleanRefreshIndicator(window);
    throw buildCmsAuthError(authFailReasonParam);
  }
  return null;
};

const handleNonAuthCall = (response: Response, window: Window) => {
  // We are here if the response is not an auth related failure i.e. 99.9% of
  //  of traffic.  We may be here if this is a successful response after we have gone
  //  around the reauth loop (which is the point of this feature).  In this second case,
  //  we should tidy away the query params that are introduced in to browser address bar.

  cleanRefreshIndicator(window);
  return response;
};

export const fullWindowReauthenticationFilter = (
  response: Response,
  window: Window,
  correlationId: string | null
) =>
  tryHandleAsFirstAuthFail(response, window, correlationId) ||
  tryHandleAsSecondAuthFail(response, window) ||
  handleNonAuthCall(response, window);

export const fetchWithFullWindowReauth = async (...args: FetchArgs) => {
  const response = await fetchWithCookies(...args);
  const correlationId = getCorrelationIdFromFetchArgs(...args);

  return fullWindowReauthenticationFilter(
    response,
    window,
    correlationId || null
  );
};
