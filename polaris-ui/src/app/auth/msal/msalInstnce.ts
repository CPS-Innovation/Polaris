import { PublicClientApplication } from "@azure/msal-browser";
import { CLIENT_ID, TENANT_ID } from "../../config";

declare global {
  var __POLARIS_PROXY_REDIRECT_URI_OVERRIDE__: string;
}

export const msalInstance = new PublicClientApplication({
  auth: {
    clientId: CLIENT_ID,
    authority: `https://login.microsoftonline.com/${TENANT_ID}`,
    redirectUri: window.__POLARIS_PROXY_REDIRECT_URI_OVERRIDE__ || "/",
    postLogoutRedirectUri: "/",
  },
});
