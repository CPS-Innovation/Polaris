import { Auth as MockAuth } from "./mock/Auth";
import { Auth as MsalAuth } from "./msal/Auth";
import { Auth as NoAuth } from "./no-auth/Auth";
import { useUserDetails as useMockUserDetails } from "./mock/useUserDetails";
import { useUserDetails as useMsalUserDetails } from "./msal/useUserDetails";
import { getAccessToken as mockGetAccessToken } from "./mock/getAccessToken";
import { getAccessToken as msalGetAccessToken } from "./msal/getAccessToken";

const NON_AUTH_SEARCH_PARAM = "automation-test-first-visit";

const isMockAuth = process.env.REACT_APP_MOCK_AUTH === "true";

const isAutomationTestFirstVisit = new URLSearchParams(
  document.location.search
).get(NON_AUTH_SEARCH_PARAM);

export const Auth = isAutomationTestFirstVisit
  ? /*
      The Cypress automation tests need to be able to open the app and set sessionStorage values
      to set the required MSAL settings (the auth id token is retrieved via a Cypress command without
      having to visit the AAD login UI).  We need to be able to let Cypress visit the app without it 
      immediately being bounced to the AAD login page.  By passing ?no-auth=true we let Cypress visit the 
      app, store the relevant settings and then go back to the home page, with the settings now in place
      (so no UI redirect to AAD login).
    */
    NoAuth
  : isMockAuth
  ? /*
      Used by the Cypress front-end integration tests (where the app is completely stiubbed away from underlying
      apis and AAD)
     */
    MockAuth
  : MsalAuth;

export const useUserDetails = isMockAuth
  ? useMockUserDetails
  : useMsalUserDetails;

export const getAccessToken = isMockAuth
  ? mockGetAccessToken
  : msalGetAccessToken;
