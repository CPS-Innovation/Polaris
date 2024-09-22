import fetchJsonp from "fetch-jsonp";
import {
  AuthFailReason,
  buildCmsAuthError,
  buildRedirectUrl,
  getCorrelationIdFromFetchArgs,
  isCmsAuthFail,
} from "./core";
import { fetchWithCookies } from "./fetch-with-cookies";
import ReactDOM from "react-dom";
import React from "react";
import { CmsAuthError } from "../../../../common/errors/CmsAuthError";
import { InSituReauthModal } from "./InSituReauthModal";

type InSituReauthResult = {
  isSuccess: boolean;
  failReason: AuthFailReason | null | undefined;
};

const inSituReauth = async (
  [endpoint, ...remainingEndpoints]: string[],
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
    `${endpoint}?cb=${jsonpCallbackFunction}`,
    correlationId
  );

  let result: InSituReauthResult;
  try {
    const reauthResult = await fetchJsonp(reauthUrl, {
      jsonpCallbackFunction,
      timeout: 10000,
    });
    result = (await reauthResult.json()) as InSituReauthResult;
  } catch (ex) {
    console.log(ex);
    result = {
      isSuccess: false,
      failReason: AuthFailReason.InSituAttemptFailed,
    };
  }

  console.log({ result, remainingEndpoints });

  return result.isSuccess || !remainingEndpoints.length
    ? // Either we are good, or we have no more endpoints to try, so the result we have is the
      //  one to return.
      result
    : inSituReauth(remainingEndpoints, correlationId);
};

const askUserToLogInToCms = async (error: CmsAuthError) => {
  return new Promise<void>((resolve) => {
    const domRoot = document.body.appendChild(document.createElement("div"));

    const handleClose = () => {
      document.body.removeChild(domRoot);
      resolve();
    };

    ReactDOM.render(
      React.createElement(InSituReauthModal, {
        error,
        handleClose,
      }),
      domRoot
    );
  });
};

export const fetchWithInSituReauth = async (
  ...args: Parameters<typeof fetch>
): Promise<Response> => {
  const response = await fetchWithCookies(...args);
  if (!isCmsAuthFail(response)) {
    // We can exit because things are ok, or if they are not it wasn't due to an auth concern
    return response;
  }

  // We are here because:
  //  1) the user has come to Polaris before logging in to CMS
  //  2) the user has been logged out by CMS
  //  3) the user has logged-in to CMS once more since we last obtained auth
  // The config may define more than one endpoint to try for auth refresh: this is useful
  //  in pre-prod environments where we may be:
  //  1) Making use of dev login and so cookies will be available on our proxy domain
  //  2) Running against multiple cin environments e.g. cin3.cps.gov.uk and cin4.cps.gov.uk
  // Lets use a convention whereby the config can supply the endpoints to try in a comma
  //  delimited list, first endpoint to try first in the list.
  const inboundEndpointsToTry =
    "/auth-refresh-termination,/api/auth-refresh-termination".split(",");
  const correlationId = getCorrelationIdFromFetchArgs(...args);
  const { isSuccess, failReason } = await inSituReauth(
    inboundEndpointsToTry,
    correlationId
  );

  if (!isSuccess) {
    // Auth is just not there in this browser so we need to ask the user to log in
    await askUserToLogInToCms(buildCmsAuthError(failReason));
  }

  // Whatever the user did, we recurse around the loop again
  return fetchWithInSituReauth(...args);
};
