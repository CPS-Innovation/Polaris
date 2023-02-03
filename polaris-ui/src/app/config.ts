export const GATEWAY_BASE_URL = process.env.REACT_APP_GATEWAY_BASE_URL!;
export const GATEWAY_SCOPE = process.env.REACT_APP_GATEWAY_SCOPE!;
export const CLIENT_ID = process.env.REACT_APP_CLIENT_ID!;
export const TENANT_ID = process.env.REACT_APP_TENANT_ID!;
export const BUILD_NUMBER = process.env.REACT_APP_BUILD_NUMBER || "development";
export const PIPELINE_POLLING_DELAY = Number(
  process.env.REACT_APP_PIPELINE_POLLING_DELAY || 2000
);
export const REAUTH_REDIRECT_URL = process.env.REACT_APP_REAUTH_REDIRECT_URL!;
// for support/diagnostics, output our env into console when deployed
//  but not during test runs, too much noise
if (process.env.NODE_ENV !== "test") {
  console.log(JSON.stringify(process.env));
}
