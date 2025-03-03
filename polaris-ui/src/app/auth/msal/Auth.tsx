import { MsalProvider } from "@azure/msal-react";
import { useEffect, useState } from "react";

import { msalInstance } from "./msalInstance";
import { PrivateBetaAuthorizationFilter } from "./PrivateBetaAuthorizationFilter";

export const Auth = ({ children }: { children: React.ReactNode }) => {
  const [isLoggedIn, setIsLoggedIn] = useState<boolean>();

  useEffect(() => {
    (async () => {
      // required so that when we are coming back from a redirect, that process is complete
      //  before we do any more auth interactions (otherwise an error is thrown)
      await msalInstance.handleRedirectPromise();

      const [account] = msalInstance.getAllAccounts();

      if (!account) {
        await msalInstance.loginRedirect({
          scopes: ["User.Read"],
        });
        return;
      }

      setIsLoggedIn(true);
    })();
  }, []);

  return isLoggedIn ? (
    <MsalProvider instance={msalInstance}>
      <PrivateBetaAuthorizationFilter
        msalInstance={msalInstance}
        window={window}
      >
        {children}
      </PrivateBetaAuthorizationFilter>
    </MsalProvider>
  ) : (
    <></>
  );
};
