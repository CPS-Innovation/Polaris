import { CmsAuthError } from "../../../common/errors/CmsAuthError";
import { CmsAuthRedirectingError } from "../../../common/errors/CmsAuthRedirectingError";
import { REAUTH_REDIRECT_URL } from "../../../config";

const REAUTHENTICATION_INDICATOR_QUERY_PARAM = "auth-refresh";
const FAIL_CORRELATION_ID_QUERY_PARAM = "fail-correlation-id";

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

const tryHandleSecondAuthFail = (response: Response, window: Window) => {
  if (isCmsAuthFail(response) && isAuthPageLoad(window)) {
    tryCleanRefreshIndicator(window);
    throw new CmsAuthError("We think you are not logged in to CMS");
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
  tryHandleSecondAuthFail(response, window) ||
  handleNonAuthCall(response, window);
