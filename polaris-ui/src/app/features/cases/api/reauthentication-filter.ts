import { CmsAuthError } from "../../../common/errors/CmsAuthError";
import { CmsAuthRedirectingError } from "../../../common/errors/CmsAuthRedirectingError";
import { REAUTH_REDIRECT_URL } from "../../../config";

const REAUTHENTICATION_INDICATOR_QUERY_PARAM = "auth-refresh";
const FAIL_CORRELATION_ID_QUERY_PARAM = "fail-correlation-id";
const AUTH_FAIL_REASON_QUERY_PARAM = "auth-fail-reason";

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
    const delimiter = window.location.href.includes("?") ? "&" : "?";
    let nextUrl = `${encodeURIComponent(
      window.location.href + delimiter + REAUTHENTICATION_INDICATOR_QUERY_PARAM
    )}`;

    if (correlationId) {
      nextUrl += `&${FAIL_CORRELATION_ID_QUERY_PARAM}=${correlationId}`;
    }

    window.location.href = `${REAUTH_REDIRECT_URL}${encodeURIComponent(
      nextUrl
    )}`;
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
      AUTH_FAIL_REASON_QUERY_PARAM
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