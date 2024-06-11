import { msalInstance } from "./msalInstance";
import {
  FEATURE_FLAG_REDACTION_LOG,
  PRIVATE_BETA_CHECK_IGNORE_USER,
  FEATURE_FLAG_FULL_SCREEN,
  FEATURE_FLAG_NOTES,
  FEATURE_FLAG_SEARCH_PII,
  PRIVATE_BETA_FEATURE_USER_GROUP,
} from "../../config";
import { useQueryParamsState } from "../../common/hooks/useQueryParamsState";
import {
  FeatureFlagQueryParams,
  FeatureFlagData,
} from "../../features/cases/domain/FeatureFlagData";
import { useUserDetails as getMockUserDetails } from "../mock/useUserDetails";
import { useUserDetails } from "../../auth";
import { useCallback, useMemo } from "react";

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

const showFeature = (
  featureFlag: boolean,
  username: string,
  queryParam: string,
  groupClaims?: string[]
) => {
  if (!featureFlag) return false;

  const isTestUser =
    window.Cypress &&
    (isAutomationTestUser(username) || isUIIntegrationTestUser(username));

  if (isTestUser && queryParam === "false") {
    return false;
  }

  if (groupClaims) {
    const isInPrivateBetaGroup = groupClaims?.includes(
      PRIVATE_BETA_FEATURE_USER_GROUP
    );
    if (!isInPrivateBetaGroup) return false;
  }
  return true;
};

export const useUserGroupsFeatureFlag = (): FeatureFlagData => {
  const { redactionLog, fullScreen, notes, searchPII } =
    useQueryParamsState<FeatureFlagQueryParams>();
  const [account] = msalInstance.getAllAccounts();
  const userDetails = useUserDetails();
  const groupClaims = account?.idTokenClaims?.groups as string[];

  const getFeatureFlags = useCallback(
    () => ({
      redactionLog: showFeature(
        FEATURE_FLAG_REDACTION_LOG,
        userDetails?.username,
        redactionLog
      ),
      fullScreen: showFeature(
        FEATURE_FLAG_FULL_SCREEN,
        userDetails?.username,
        fullScreen
      ),
      notes: showFeature(FEATURE_FLAG_NOTES, userDetails?.username, notes),
      searchPII: showFeature(
        FEATURE_FLAG_SEARCH_PII,
        userDetails?.username,
        searchPII,
        groupClaims
      ),
    }),
    []
  );
  return useMemo(() => getFeatureFlags(), [getFeatureFlags]);
};
