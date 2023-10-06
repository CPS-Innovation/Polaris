export const GATEWAY_BASE_URL = process.env.REACT_APP_GATEWAY_BASE_URL!;
export const GATEWAY_SCOPE = process.env.REACT_APP_GATEWAY_SCOPE!;
export const CLIENT_ID = process.env.REACT_APP_CLIENT_ID!;
export const TENANT_ID = process.env.REACT_APP_TENANT_ID!;
export const BUILD_NUMBER = process.env.REACT_APP_BUILD_NUMBER || "development";
export const SURVEY_LINK = process.env.REACT_APP_SURVEY_LINK;

/*
  To avoid public routing, we send out app insights traffic through on our own domain via the proxy.
  In a given environment the application is accessed via different domain names depending on whether
  we are on the VPN or a CPS device etc.  That means we need to specify IngestionEndpoint
  and LiveEndpoint dynamically based on the current domain.
*/
export const AI_CONNECTION_STRING =
  `InstrumentationKey=${process.env.REACT_APP_AI_KEY};` +
  `IngestionEndpoint=${window.location.origin};` +
  `LiveEndpoint=${window.location.origin}`;

export const REPORT_ISSUE = process.env.REACT_APP_REPORT_ISSUE === "true";
export const PIPELINE_POLLING_DELAY = Number(
  process.env.REACT_APP_PIPELINE_POLLING_DELAY || 2000
);
export const REAUTH_REDIRECT_URL = process.env.REACT_APP_REAUTH_REDIRECT_URL!;

export const PRIVATE_BETA_USER_GROUP =
  process.env.REACT_APP_PRIVATE_BETA_USER_GROUP!;

export const PRIVATE_BETA_SIGN_UP_URL =
  process.env.REACT_APP_PRIVATE_BETA_SIGN_UP_URL!;

export const PRIVATE_BETA_CHECK_IGNORE_USER =
  process.env.REACT_APP_PRIVATE_BETA_CHECK_IGNORE_USER;

export const IS_REDACTION_SERVICE_OFFLINE =
  process.env.REACT_APP_IS_REDACTION_SERVICE_OFFLINE === "true";

// for support/diagnostics, output our env into console when deployed
//  but not during test runs, too much noise
if (process.env.NODE_ENV !== "test") {
  console.log(JSON.stringify(process.env));
}

export const IS_REDACTION_SERVICE_OFFLINE =
  process.env.REACT_APP_IS_REDACTION_SERVICE_OFFLINE === "true";
