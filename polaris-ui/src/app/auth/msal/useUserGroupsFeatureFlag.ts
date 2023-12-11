import { msalInstance } from "./msalInstance";
import {
  PRIVATE_BETA_REDACTION_LOG_FEATURE,
  FEATURE_FLAG_REDACTION_LOG,
  PRIVATE_BETA_CHECK_IGNORE_USER,
} from "../../config";

const showRedactionLogFeature = (groupClaims: string[]) => {
  const isInRedactionLogFeature = !!groupClaims?.includes(
    PRIVATE_BETA_REDACTION_LOG_FEATURE
  );

  const canProceedOnNoFeatureGroupInConfig = !(
    PRIVATE_BETA_REDACTION_LOG_FEATURE &&
    PRIVATE_BETA_REDACTION_LOG_FEATURE.length
  );

  const showRedactionLog =
    (isInRedactionLogFeature && FEATURE_FLAG_REDACTION_LOG) ||
    canProceedOnNoFeatureGroupInConfig;
  return showRedactionLog;
};

export const useUserGroupsFeatureFlag = () => {
  const [account] = msalInstance.getAllAccounts();
  const username = account?.username;
  const groupClaims = account?.idTokenClaims?.groups as string[];

  const canProceedOnAutomationTestRun =
    username &&
    PRIVATE_BETA_CHECK_IGNORE_USER &&
    username.toLocaleLowerCase() ===
      PRIVATE_BETA_CHECK_IGNORE_USER.toLocaleLowerCase();

  return {
    redactionLog:
      canProceedOnAutomationTestRun || showRedactionLogFeature(groupClaims),
  };
};
