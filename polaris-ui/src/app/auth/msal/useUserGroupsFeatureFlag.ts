import { msalInstance } from "./msalInstance";
import {
  FEATURE_FLAG_REDACTION_LOG,
  FEATURE_FLAG_FULL_SCREEN,
  FEATURE_FLAG_NOTES,
  FEATURE_FLAG_SEARCH_PII,
  FEATURE_FLAG_RENAME_DOCUMENT,
  FEATURE_FLAG_RECLASSIFY,
  PRIVATE_BETA_FEATURE_USER_GROUP,
  PRIVATE_BETA_FEATURE_USER_GROUP2,
  FEATURE_FLAG_EXTERNAL_REDIRECT,
  PRIVATE_BETA_FEATURE_USER_GROUP3,
  PRIVATE_BETA_CHECK_IGNORE_USER,
} from "../../config";
import { useQueryParamsState } from "../../common/hooks/useQueryParamsState";
import { useUserDetails as getMockUserDetails } from "../mock/useUserDetails";
import {
  FeatureFlagQueryParams,
  FeatureFlagData,
} from "../../features/cases/domain/FeatureFlagData";
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

export const isThisATestUser = (username: string) =>
  !!window.Cypress &&
  (isAutomationTestUser(username) || isUIIntegrationTestUser(username));

const showFeature = (
  featureFlag: boolean,
  username: string,
  queryParam: string,
  groupClaims?: { groupKey: string; groups: string[] }
) => {
  if (!featureFlag) return false;

  const isTestUser = isThisATestUser(username);

  if (isTestUser && queryParam === "false") return false;
  //bypassing group claims for cypress test users, if the featureFlag is true
  if (isTestUser && queryParam === "true") return true;

  if (groupClaims?.groups) {
    const isInPrivateBetaGroup = groupClaims?.groups.includes(
      groupClaims.groupKey
    );
    if (!isInPrivateBetaGroup) return false;
  }
  return true;
};

export const useUserGroupsFeatureFlag = (): FeatureFlagData => {
  const {
    redactionLog,
    fullScreen,
    notes,
    searchPII,
    renameDocument,
    reclassify,
    externalRedirect,
  } = useQueryParamsState<FeatureFlagQueryParams>();
  const [account] = msalInstance.getAllAccounts();
  const userDetails = useUserDetails();
  const groupClaims = (account?.idTokenClaims?.groups as string[]) ?? [];

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
        { groups: groupClaims, groupKey: PRIVATE_BETA_FEATURE_USER_GROUP }
      ),
      renameDocument: showFeature(
        FEATURE_FLAG_RENAME_DOCUMENT,
        userDetails?.username,
        renameDocument
      ),
      externalRedirect: showFeature(
        FEATURE_FLAG_EXTERNAL_REDIRECT,
        userDetails?.username,
        externalRedirect,
        { groups: groupClaims, groupKey: PRIVATE_BETA_FEATURE_USER_GROUP3 }
      ),
      reclassify: showFeature(
        FEATURE_FLAG_RECLASSIFY,
        userDetails?.username,
        reclassify,
        { groups: groupClaims, groupKey: PRIVATE_BETA_FEATURE_USER_GROUP2 }
      ),
    }),
    []
  );
  return useMemo(() => getFeatureFlags(), [getFeatureFlags]);
};
