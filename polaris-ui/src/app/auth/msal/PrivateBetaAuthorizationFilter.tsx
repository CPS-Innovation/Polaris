import { IPublicClientApplication } from "@azure/msal-browser";
import { FC } from "react";
import {
  PRIVATE_BETA_SIGN_UP_URL,
  PRIVATE_BETA_USER_GROUP,
  PRIVATE_BETA_CHECK_IGNORE_USER,
} from "../../config";
type Props = {
  msalInstance: IPublicClientApplication;
  window: Window;
};

export const PrivateBetaAuthorizationFilter: FC<Props> = ({
  msalInstance,
  children,
  window: windowViaProps,
}) => {
  const proceedAuthorized = () => <>{children}</>;
  const proceedNotAuthorized = () => {
    windowViaProps.location.href = PRIVATE_BETA_SIGN_UP_URL;
    return null;
  };

  // if no group is configured, then we allow all users
  if (!PRIVATE_BETA_USER_GROUP) {
    return proceedAuthorized();
  }

  const [account] = msalInstance.getAllAccounts();

  if (
    account.username &&
    PRIVATE_BETA_CHECK_IGNORE_USER &&
    account.username === PRIVATE_BETA_CHECK_IGNORE_USER
  ) {
    return proceedAuthorized();
  }

  const groupClaims = account?.idTokenClaims?.groups as string[];
  return groupClaims && groupClaims.includes(PRIVATE_BETA_USER_GROUP)
    ? proceedAuthorized()
    : proceedNotAuthorized();
};
