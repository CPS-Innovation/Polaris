import { CmsError } from "../../../common/errors/CmsAuthError";
import { REAUTH_REDIRECT_URL } from "../../../config";

const REAUTHENTICATION_INDICATOR_QUERY_PARAM = "auth-refresh";

const isCmsAuthFail = (response: Response) => response.status === 403;

const isAuthPageLoad = (window: Window) =>
  window.location.href.includes(REAUTHENTICATION_INDICATOR_QUERY_PARAM);

const tryHandleFirstAuthFail = (response: Response, window: Window) => {
  if (isCmsAuthFail(response) && !isAuthPageLoad(window)) {
    const delimiter = window.location.href.includes("?") ? "&" : "?";

    const nextUrl = `${REAUTH_REDIRECT_URL}${encodeURIComponent(
      window.location.href + delimiter + REAUTHENTICATION_INDICATOR_QUERY_PARAM
    )}`;
    window.location.href = nextUrl;
    // keep typing happy by returning the response, although functionally useless
    return response;
  }
  return null;
};

const tryHandleSecondAuthFail = (response: Response, window: Window) => {
  if (isCmsAuthFail(response) && isAuthPageLoad(window)) {
    throw new CmsError("We think you are not logged in to CMS");
  }
  return null;
};

const handleNonAuthCall = (response: Response, window: Window) => {
  // clean the indicator from the browser address bar
  if (window.location.href.includes(REAUTHENTICATION_INDICATOR_QUERY_PARAM)) {
    const nextUrl = window.location.href.replace(
      new RegExp(`[?|&]${REAUTHENTICATION_INDICATOR_QUERY_PARAM}`),
      ""
    );

    window.history.replaceState(null, "", nextUrl);
  }
  return response;
};

export const reauthenticationFilter = (response: Response, window: Window) =>
  tryHandleFirstAuthFail(response, window) ||
  tryHandleSecondAuthFail(response, window) ||
  handleNonAuthCall(response, window);
