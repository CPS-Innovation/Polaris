import { msalInstance } from "./msal-instance";

export const ensureMsalLoggedIn = async () => {
  await msalInstance.handleRedirectPromise();

  const [account] = msalInstance.getAllAccounts();

  if (!account) {
    await msalInstance.loginRedirect({
      scopes: ["User.Read"],
    });
  }
};
