import { msalInstance } from "./msalInstance";
import {
  PRIVATE_BETA_REDACTION_LOG_USER_GROUP,
  REDACTION_LOG_USER_GROUP,
  FEATURE_FLAG_REDACTION_LOG,
  PRIVATE_BETA_CHECK_IGNORE_USER,
} from "../../config";
import { useQueryParamsState } from "../../common/hooks/useQueryParamsState";
import {
  FeatureFlagQueryParams,
  FeatureFlagData,
} from "../../features/cases/domain/FeatureFlagData";
import { useUserDetails as getMockUserDetails } from "../mock/useUserDetails";
import { useUserDetails } from "../../auth";

const isAutomationTestUser = (username: string) => {
  return !!(
    username &&
    PRIVATE_BETA_CHECK_IGNORE_USER &&
    username.toLowerCase() === PRIVATE_BETA_CHECK_IGNORE_USER.toLowerCase()
  );
};

const isUIIntegrationTestUser = (username: string) => {
  return !!(
    username &&
    username.toLowerCase() === getMockUserDetails().username.toLowerCase()
  );
};

const showRedactionLogFeature = (
  groupClaims: string[],
  username: string,
  queryParam: string
) => {
  if (!FEATURE_FLAG_REDACTION_LOG) {
    return false;
  }

  const isInCypressQueryParamFeatureFlag =
    queryParam === "true" &&
    !!window.Cypress &&
    (isAutomationTestUser(username) || isUIIntegrationTestUser(username));

  const isInPrivateBetaGroup = !!groupClaims?.includes(
    PRIVATE_BETA_REDACTION_LOG_USER_GROUP
  );

  const isInRedactionLogGroup = !!groupClaims?.includes(
    REDACTION_LOG_USER_GROUP
  );

  const canProceedBasedOnADGroups =
    isInPrivateBetaGroup && isInRedactionLogGroup;

  const canProceedOnNoFeatureGroupInConfig =
    !PRIVATE_BETA_REDACTION_LOG_USER_GROUP?.length;

  return (
    canProceedBasedOnADGroups ||
    canProceedOnNoFeatureGroupInConfig ||
    isInCypressQueryParamFeatureFlag
  );
};

export const useUserGroupsFeatureFlag = (): FeatureFlagData => {
  const { redactionLog } = useQueryParamsState<FeatureFlagQueryParams>();
  console.log("msalInstance>>>", msalInstance);
  console.log("msalInstance>>>", msalInstance.getAllAccounts());
  const [account] = msalInstance.getAllAccounts();
  const userDetails = useUserDetails();
  const groupClaims = account?.idTokenClaims?.groups as string[];

  return {
    redactionLog: showRedactionLogFeature(
      groupClaims,
      userDetails?.username,
      redactionLog
    ),
  };
};
