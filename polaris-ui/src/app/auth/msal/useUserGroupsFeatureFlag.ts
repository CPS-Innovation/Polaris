import { msalInstance } from "./msalInstance";
import {
  PRIVATE_BETA_REDACTION_LOG_USER_GROUP,
  REDACTION_LOG_USER_GROUP,
  FEATURE_FLAG_REDACTION_LOG,
  PRIVATE_BETA_CHECK_IGNORE_USER,
} from "../../config";

const isAutomationTestUser = (username: string) => {
  return (
    username &&
    PRIVATE_BETA_CHECK_IGNORE_USER &&
    username.toLocaleLowerCase() ===
      PRIVATE_BETA_CHECK_IGNORE_USER.toLocaleLowerCase()
  );
};

const showRedactionLogFeature = (groupClaims: string[], username: string) => {
  const isInPrivateBetaGroup = !!groupClaims?.includes(
    PRIVATE_BETA_REDACTION_LOG_USER_GROUP
  );

  const isInRedactionLogGroup = !!groupClaims?.includes(
    REDACTION_LOG_USER_GROUP
  );

  const canProceedBasedOnADGroups =
    isInPrivateBetaGroup && isInRedactionLogGroup && FEATURE_FLAG_REDACTION_LOG;

  const canProceedOnNoFeatureGroupInConfig =
    !(
      PRIVATE_BETA_REDACTION_LOG_USER_GROUP &&
      PRIVATE_BETA_REDACTION_LOG_USER_GROUP.length
    ) && FEATURE_FLAG_REDACTION_LOG;

  const canProceedOnAutomationTestRun =
    isAutomationTestUser(username) && FEATURE_FLAG_REDACTION_LOG;

  return (
    canProceedBasedOnADGroups ||
    canProceedOnNoFeatureGroupInConfig ||
    canProceedOnAutomationTestRun
  );
};

export const useUserGroupsFeatureFlag = () => {
  const [account] = msalInstance.getAllAccounts();
  const username = account?.username;
  const groupClaims = account?.idTokenClaims?.groups as string[];

  return {
    redactionLog: showRedactionLogFeature(groupClaims, username),
  };
};
