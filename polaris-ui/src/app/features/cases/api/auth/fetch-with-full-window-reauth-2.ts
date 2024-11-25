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
  FAIL_CORRELATION_ID_QUERY_PARAM,
  FetchArgs,
  getCorrelationIdFromFetchArgs,
  isCmsAuthFail,
  REAUTHENTICATION_INDICATOR_QUERY_PARAM,
} from "./core";
import { fetchWithCookies } from "./fetch-with-cookies";

const cleanRefreshIndicatorsFromUrl = (existingUrl: string) => {
  let url = new URL(existingUrl);
  url.searchParams.delete(REAUTHENTICATION_INDICATOR_QUERY_PARAM);
  url.searchParams.delete(AUTH_FAIL_REASON_QUERY_PARAM);
  url.searchParams.delete(FAIL_CORRELATION_ID_QUERY_PARAM);
  return url.toString();
};

const cleanRefreshIndicatorsFromAddress = (window: Window) => {
  // clean the indicator from the browser address bar
  if (window.location.href.includes(REAUTHENTICATION_INDICATOR_QUERY_PARAM)) {
    window.history.replaceState(
      null,
      "",
      cleanRefreshIndicatorsFromUrl(window.location.href)
    );
  }
};

const buildRedirectUrl = (
  window: Window,
  outboundUrlIndex: number,
  correlationId: string | null
) => {
  const outboundUrlsToTry = window.Cypress
    ? [REAUTH_REDIRECT_URL_OUTBOUND_E2E]
    : REAUTH_REDIRECT_URL_OUTBOUND.split(",");

  if (outboundUrlIndex > outboundUrlsToTry.length - 1) {
    return undefined;
  }

  const cleanedCurrentAddress = cleanRefreshIndicatorsFromUrl(
    window.location.href
  );

  return assembleRedirectUrl({
    outboundUrl: outboundUrlsToTry[outboundUrlIndex],
    inboundUrl: REAUTH_REDIRECT_URL_INBOUND,
    terminationUrl:
      `${cleanedCurrentAddress}${
        cleanedCurrentAddress.includes("?") ? "&" : "?"
      }${REAUTHENTICATION_INDICATOR_QUERY_PARAM}=${outboundUrlIndex}` +
      // it is also useful to pass the correlation id back to the UI
      "&" +
      FAIL_CORRELATION_ID_QUERY_PARAM +
      "=" +
      correlationId,
    correlationId,
  });
};

const navigateAndStopExecution = (url: string) => {
  window.location.href = url;

  // stop any follow-on logic occurring
  throw new CmsAuthRedirectingError();
};

export const fetchWithFullWindowReauth = async (...args: FetchArgs) => {
  const response = await fetchWithCookies(...args);
  const correlationId = getCorrelationIdFromFetchArgs(...args);

  if (isCmsAuthFail(response)) {
    // If this response is an auth fail, we will be sending the entire browser window
    //  around the re-auth loop by setting `window.location.href` and then throwing an
    //  error to halt our code execution and prevent any follow-on functionality from firing
    //  inadvertently.
    const redirectUrl = buildRedirectUrl(window, 0, correlationId)!;
    navigateAndStopExecution(redirectUrl);
  }

  return response;
};

export const handleAuthRelatedReload = (window: Window) => {
  const urlParams = new URLSearchParams(window.location.search);

  const authFailReasonParam = urlParams.get(
    AUTH_FAIL_REASON_QUERY_PARAM
  ) as AuthFailReason;

  if (authFailReasonParam === null) {
    // If there is not auth fail reason query param then either
    //  - this is a normal app load, nothing to see here
    //  - this is a reauth reload but everything has gone ok with the reauth, nothing to see here
    cleanRefreshIndicatorsFromAddress(window);
    return;
  }

  // So we have tried a reauth and have come back here with a fail reason param.  #28346 gives us
  //  the ability to try more than one `/polaris` endpoint, so lets see if our config has more than
  //  one outbound url in the config setting
  const authReloadIndex = urlParams.get(REAUTHENTICATION_INDICATOR_QUERY_PARAM);
  const correlationId = urlParams.get(FAIL_CORRELATION_ID_QUERY_PARAM);

  const nextRedirectUrl = buildRedirectUrl(
    window,
    Number(authReloadIndex) + 1,
    correlationId
  );

  if (nextRedirectUrl) {
    // Yes, there is another outbound url to try
    navigateAndStopExecution(nextRedirectUrl);
  } else {
    // No, there are no more outbound urls to try, so let's throw the final error to the user
    //cleanRefreshIndicators(window);
    throw buildCmsAuthError(authFailReasonParam);
  }
};

handleAuthRelatedReload(window);
