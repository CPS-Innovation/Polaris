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

const showRedactionLogFeature = (username: string, queryParam: string) => {
  if (!FEATURE_FLAG_REDACTION_LOG) {
    return false;
  }

  const isInCypressQueryParamFeatureFlag =
    queryParam === "false" &&
    window.Cypress &&
    (isAutomationTestUser(username) || isUIIntegrationTestUser(username));

  if (isInCypressQueryParamFeatureFlag) return false;

  return true;
};

const showFullScreenFeature = (username: string, queryParam: string) => {
  if (!FEATURE_FLAG_FULL_SCREEN) {
    return false;
  }
  const isInCypressQueryParamFeatureFlag =
    queryParam === "false" &&
    window.Cypress &&
    (isAutomationTestUser(username) || isUIIntegrationTestUser(username));
  if (isInCypressQueryParamFeatureFlag) return false;

  return true;
};

const showNotesFeature = (
  username: string,
  queryParam: string,
  groupClaims: string[]
) => {
  if (!FEATURE_FLAG_NOTES) {
    return false;
  }

  const isTestUser =
    window.Cypress &&
    (isAutomationTestUser(username) || isUIIntegrationTestUser(username));

  if (isTestUser) {
    if (queryParam === "true") {
      return true;
    }
    if (queryParam === "false") {
      return false;
    }
  }

  const isInPrivateBetaGroup = !!groupClaims?.includes(
    PRIVATE_BETA_FEATURE_USER_GROUP
  );

  if (!isInPrivateBetaGroup) return false;
  return true;
};

const showSearchPIIFeature = (
  username: string,
  queryParam: string,
  groupClaims: string[]
) => {
  if (!FEATURE_FLAG_SEARCH_PII) {
    return false;
  }

  const isTestUser =
    window.Cypress &&
    (isAutomationTestUser(username) || isUIIntegrationTestUser(username));

  if (isTestUser) {
    if (queryParam === "true") {
      return true;
    }
    if (queryParam === "false") {
      return false;
    }
  }

  const isInPrivateBetaGroup = !!groupClaims?.includes(
    PRIVATE_BETA_FEATURE_USER_GROUP
  );

  if (!isInPrivateBetaGroup) return false;
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
      redactionLog: showRedactionLogFeature(
        userDetails?.username,
        redactionLog
      ),
      fullScreen: showFullScreenFeature(userDetails?.username, fullScreen),
      notes: showNotesFeature(userDetails?.username, notes, groupClaims),
      searchPII: showSearchPIIFeature(
        userDetails?.username,
        searchPII,
        groupClaims
      ),
    }),
    []
  );

  return useMemo(() => getFeatureFlags(), [getFeatureFlags]);
};
