import { PublicClientApplication } from "@azure/msal-browser";
import { CLIENT_ID, TENANT_ID } from "../../config";

export const msalInstance = new PublicClientApplication({
  auth: {
    clientId: CLIENT_ID,
    authority: `https://login.microsoftonline.com/${TENANT_ID}`,
    redirectUri: "/",
    postLogoutRedirectUri: "/",
  },
});
