import {
  InteractionRequiredAuthError,
  SilentRequest,
} from "@azure/msal-browser";
import { msalInstance } from "./msalInstnce";

export const getAccessToken = async (scopes: string[]) => {
  const [account] = msalInstance.getAllAccounts();

  const request = {
    scopes,
    account,
  } as SilentRequest;

  try {
    const { accessToken } = await msalInstance.acquireTokenSilent(request);
    return accessToken;
  } catch (error) {
    if (error instanceof InteractionRequiredAuthError) {
      await msalInstance.acquireTokenRedirect(request);
    }
    return String(); // so the return type is always string
  }
};
