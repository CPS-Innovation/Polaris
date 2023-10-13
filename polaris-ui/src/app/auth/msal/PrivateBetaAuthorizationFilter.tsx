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

const proceedNotAuthorized = (window: Window) => {
  window.location.href = PRIVATE_BETA_SIGN_UP_URL;
  return null;
};

export const PrivateBetaAuthorizationFilter: FC<Props> = ({
  msalInstance,
  children,
  window: windowViaProps,
}) => {
  const [account] = msalInstance.getAllAccounts();
  const username = account?.username;
  const groupClaims = account?.idTokenClaims?.groups as string[];

  // DANGER: some minification problem here:
  //  `!PRIVATE_BETA_USER_GROUP` is true in production even if PRIVATE_BETA_USER_GROUP is ""
  const canProceedOnNoGroupInConfig = !(
    PRIVATE_BETA_USER_GROUP && PRIVATE_BETA_USER_GROUP.length
  );

  const canProceedOnAutomationTestRun =
    username &&
    PRIVATE_BETA_CHECK_IGNORE_USER &&
    username.toLocaleLowerCase() ===
      PRIVATE_BETA_CHECK_IGNORE_USER.toLocaleLowerCase();

  const canProceedOnGroupMembership = !!groupClaims?.includes(
    PRIVATE_BETA_USER_GROUP
  );

  return canProceedOnNoGroupInConfig ||
    canProceedOnAutomationTestRun ||
    canProceedOnGroupMembership ? (
    <>{children}</>
  ) : (
    proceedNotAuthorized(windowViaProps)
  );
};
