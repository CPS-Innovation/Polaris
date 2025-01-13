export const TENANT_ID = "00dd0d1d-d7e6-4338-ac51-565339c7088c";
export const CLIENT_ID = "3649c1c8-00cf-4b8f-a671-304bc074937c";
export const GATEWAY_SCOPE = "https://CPSGOVUK.onmicrosoft.com/fa-polaris-dev-gateway/user_impersonation";
//export const GATEWAY_BASE_URL = "https://polaris-dev-cmsproxy.azurewebsites.net";
// export const REAUTH_REDIRECT_URL_OUTBOUND = "https://polaris-dev-cmsproxy.azurewebsites.net/polaris";
// export const REAUTH_REDIRECT_URL_INBOUND = "https://polaris-dev-cmsproxy.azurewebsites.net/auth-refresh-inbound";
// export const REAUTH_IN_SITU_TERMINATION_URL = "https://polaris-dev-cmsproxy.azurewebsites.net/auth-refresh-termination";
export const GATEWAY_BASE_URL = "http://localhost:7075";
export const REAUTH_REDIRECT_URL_OUTBOUND = "http://localhost:7071/api/polaris";
export const REAUTH_REDIRECT_URL_INBOUND = "/api/init";
export const REAUTH_IN_SITU_TERMINATION_URL = "/api/auth-refresh-termination";

export const CORRELATION_ID = "Correlation-Id";
export const EXPERIMENTAL_REQUEST_CMSCOOKIES_RETURNED_TO_UI_PARAM_NAME = "request-cms-auth-values-returned-to-ui";
export const CMS_AUTH_VALUES_SESSION_KEY = "Cms-Auth-Values";
