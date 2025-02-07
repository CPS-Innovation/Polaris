import fetchJsonp from "fetch-jsonp";
import {
  assembleRedirectUrl,
  AuthFailReason,
  buildCmsAuthError,
  FetchArgs,
  getCorrelationIdFromFetchArgs,
  isCmsAuthFail,
} from "./core";
import { fetchWithCookies } from "./fetch-with-cookies";
import { createRoot } from "react-dom/client";
import React from "react";
import { CmsAuthError } from "../../../../common/errors/CmsAuthError";
import { InSituReauthModal } from "./InSituReauthModal";
import {
  REAUTH_IN_SITU_TERMINATION_URL,
  REAUTH_REDIRECT_URL_INBOUND,
  REAUTH_REDIRECT_URL_OUTBOUND,
  REAUTH_REDIRECT_URL_OUTBOUND_E2E,
} from "../../../../config";

type InSituReauthResult = {
  isSuccess: boolean;
  failReason: AuthFailReason | null | undefined;
};

const buildRedirectUrls = (
  window: Window,
  jsonpCallbackFunction: string,
  correlationId: string | null
) => {
  // In the in-situ reauth flow, we can try multiple outbound routes to try and find auth.  This
  //  helps with running against multiple cin* environments from one CWA environment.
  const outboundUrlsToTry = window.Cypress
    ? [REAUTH_REDIRECT_URL_OUTBOUND_E2E]
    : REAUTH_REDIRECT_URL_OUTBOUND.split(",");

  return outboundUrlsToTry.map((outboundUrl) =>
    assembleRedirectUrl({
      outboundUrl,
      inboundUrl: REAUTH_REDIRECT_URL_INBOUND,
      terminationUrl: `${REAUTH_IN_SITU_TERMINATION_URL}?cb=${jsonpCallbackFunction}`,
      correlationId,
    })
  );
};

const tryReauth = async (
  reauthUrls: string[],
  jsonpCallbackFunction: string
): Promise<InSituReauthResult> => {
  const [reauthUrl, ...remainingReauthUrls] = reauthUrls;

  let result: InSituReauthResult;
  try {
    const reauthResult = await fetchJsonp(reauthUrl, {
      jsonpCallbackFunction,
      timeout: 50000,
    });
    result = (await reauthResult.json()) as InSituReauthResult;
  } catch (error) {
    result = {
      isSuccess: false,
      failReason: AuthFailReason.InSituAttemptFailed,
    };
  }

  return result.isSuccess || !remainingReauthUrls.length
    ? result
    : tryReauth(remainingReauthUrls, jsonpCallbackFunction);
};

const inSituReauth = async (
  correlationId: string | null
): Promise<InSituReauthResult> => {
  // We cannot quite use the native functionality of `fetch-jsonp` as we
  // should be using the `r=...` parameter for our return URL. So we mimic
  // FWIW by using the same terminology as the library uses and assemble the callback
  // param ourselves.
  const jsonpCallbackFunction = `jsonp_${Date.now()}_${Math.ceil(
    Math.random() * 100000
  )}`;

  // We are here because:
  //  1) the user has come to Polaris before logging in to CMS
  //  2) the user has been logged out by CMS
  //  3) the user has logged-in to CMS once more since we last obtained auth
  // The config may define more than one endpoint to try for auth refresh: this is useful
  //  in pre-prod environments where we may be:
  //  1) Making use of dev login and so cookies will be available on our proxy domain
  //  2) Running against multiple cin environments e.g. cin3.cps.gov.uk and cin4.cps.gov.uk
  const reauthUrls = buildRedirectUrls(
    window,
    jsonpCallbackFunction,
    correlationId
  );

  return tryReauth(reauthUrls, jsonpCallbackFunction);
};

const askUserToLogInToCms = async (error: CmsAuthError) => {
  return new Promise<void>((resolve) => {
    const domRoot = document.body.appendChild(document.createElement("div"));
    const root = createRoot(domRoot!);

    const handleClose = () => {
      document.body.removeChild(domRoot);
      resolve();
    };

    root.render(
      React.createElement(InSituReauthModal, {
        error,
        handleClose,
      })
    );
  });
};

const tryObtainAuth = async (correlationId: string | null) => {
  const { isSuccess, failReason } = await inSituReauth(correlationId);

  if (!isSuccess) {
    // Auth is just not there in this browser so we need to ask the user to log in
    await askUserToLogInToCms(buildCmsAuthError(failReason));
  }
};

export const fetchWithInSituReauth = async (
  ...args: FetchArgs
): Promise<Response> => {
  const response = await fetchWithCookies(...args);
  if (!isCmsAuthFail(response)) {
    // We can exit because things are ok, or if they are not it wasn't due to an auth concern
    return response;
  }

  const correlationId = getCorrelationIdFromFetchArgs(...args);
  await tryObtainAuth(correlationId);

  // Whatever the user did, we recurse around the loop again
  return fetchWithInSituReauth(...args);
};

export const fetchWithProactiveInSituReauth = async (...args: FetchArgs) => {
  await tryObtainAuth(null);
  return fetchWithInSituReauth(...args);
};
