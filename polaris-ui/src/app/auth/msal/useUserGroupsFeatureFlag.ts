import { msalInstance } from "./msalInstance";
import {
  FEATURE_FLAG_REDACTION_LOG,
  PRIVATE_BETA_CHECK_IGNORE_USER,
  FEATURE_FLAG_FULL_SCREEN,
  FEATURE_FLAG_NOTES,
  FEATURE_FLAG_SEARCH_PII,
  FEATURE_FLAG_RENAME_DOCUMENT,
  FEATURE_FLAG_RECLASSIFY,
  FEATURE_FLAG_PAGE_DELETE,
  FEATURE_FLAG_PAGE_ROTATE,
  FEATURE_FLAG_STATE_RETENTION,
  FEATURE_FLAG_GLOBAL_NAV,
  PRIVATE_BETA_FEATURE_USER_GROUP,
  FEATURE_FLAG_EXTERNAL_REDIRECT_CASE_REVIEW_APP,
  FEATURE_FLAG_EXTERNAL_REDIRECT_BULK_UM_APP,
  PRIVATE_BETA_FEATURE_USER_GROUP2,
  PRIVATE_BETA_FEATURE_USER_GROUP3,
  PRIVATE_BETA_FEATURE_USER_GROUP4,
  PRIVATE_BETA_FEATURE_USER_GROUP5,
  PRIVATE_BETA_FEATURE_USER_GROUP6,
  FEATURE_FLAG_BACKGROUND_PIPELINE_REFRESH,
  FEATURE_FLAG_USED_DOCUMENT_STATE,
  FEATURE_FLAG_REDACTION_TOGGLE_COPY_BUTTON,
  FEATURE_FLAG_DOCUMENT_NAME_SEARCH,
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

const shouldShowFeature = (
  isFeatureFlagOnInApp: boolean,
  username: string,
  queryParam: string,
  groupClaims?: { groupKey: string; groups: string[] }
) => {
  if (!isFeatureFlagOnInApp) {
    return false;
  }

  const isTestUserWithQueryParam =
    !!window.Cypress &&
    (isAutomationTestUser(username) || isUIIntegrationTestUser(username)) &&
    (queryParam === "true" || queryParam === "false");

  if (isTestUserWithQueryParam) {
    return queryParam === "true";
  }

  const shouldConsiderGroupClaims = !!groupClaims;

  return shouldConsiderGroupClaims
    ? groupClaims.groups.includes(groupClaims.groupKey)
    : true;
};

export const useUserGroupsFeatureFlag = (): FeatureFlagData => {
  const {
    redactionLog,
    fullScreen,
    notes,
    searchPII,
    renameDocument,
    reclassify,
    externalRedirectCaseReviewApp,
    externalRedirectBulkUmApp,
    pageDelete,
    pageRotate,
    notifications,
    isUnused,
    stateRetention,
    globalNav,
    copyRedactionTextButton,
    documentNameSearch,
  } = useQueryParamsState<FeatureFlagQueryParams>();
  const [account] = msalInstance.getAllAccounts();
  const userDetails = useUserDetails();
  const groupClaims = (account?.idTokenClaims?.groups as string[]) ?? [];

  const getFeatureFlags = useCallback(
    () => ({
      redactionLog: shouldShowFeature(
        FEATURE_FLAG_REDACTION_LOG,
        userDetails?.username,
        redactionLog
      ),
      fullScreen: shouldShowFeature(
        FEATURE_FLAG_FULL_SCREEN,
        userDetails?.username,
        fullScreen
      ),
      notes: shouldShowFeature(
        FEATURE_FLAG_NOTES,
        userDetails?.username,
        notes
      ),
      searchPII: shouldShowFeature(
        FEATURE_FLAG_SEARCH_PII,
        userDetails?.username,
        searchPII,
        { groups: groupClaims, groupKey: PRIVATE_BETA_FEATURE_USER_GROUP }
      ),
      renameDocument: shouldShowFeature(
        FEATURE_FLAG_RENAME_DOCUMENT,
        userDetails?.username,
        renameDocument
      ),
      externalRedirectCaseReviewApp: shouldShowFeature(
        FEATURE_FLAG_EXTERNAL_REDIRECT_CASE_REVIEW_APP,
        userDetails?.username,
        externalRedirectCaseReviewApp,
        { groups: groupClaims, groupKey: PRIVATE_BETA_FEATURE_USER_GROUP3 }
      ),
      externalRedirectBulkUmApp: shouldShowFeature(
        FEATURE_FLAG_EXTERNAL_REDIRECT_BULK_UM_APP,
        userDetails?.username,
        externalRedirectBulkUmApp,
        { groups: groupClaims, groupKey: PRIVATE_BETA_FEATURE_USER_GROUP4 }
      ),
      reclassify: shouldShowFeature(
        FEATURE_FLAG_RECLASSIFY,
        userDetails?.username,
        reclassify
      ),
      pageDelete: shouldShowFeature(
        FEATURE_FLAG_PAGE_DELETE,
        userDetails?.username,
        pageDelete
      ),
      pageRotate: shouldShowFeature(
        FEATURE_FLAG_PAGE_ROTATE,
        userDetails?.username,
        pageRotate
      ),
      notifications: shouldShowFeature(
        FEATURE_FLAG_BACKGROUND_PIPELINE_REFRESH,
        userDetails?.username,
        notifications,
        { groups: groupClaims, groupKey: PRIVATE_BETA_FEATURE_USER_GROUP2 }
      ),
      isUnused: shouldShowFeature(
        FEATURE_FLAG_USED_DOCUMENT_STATE,
        userDetails?.username,
        isUnused,
        { groups: groupClaims, groupKey: PRIVATE_BETA_FEATURE_USER_GROUP2 }
      ),
      stateRetention: shouldShowFeature(
        FEATURE_FLAG_STATE_RETENTION,
        userDetails?.username,
        stateRetention
      ),
      globalNav: shouldShowFeature(
        FEATURE_FLAG_GLOBAL_NAV,
        userDetails?.username,
        globalNav
      ),
      copyRedactionTextButton: shouldShowFeature(
        FEATURE_FLAG_REDACTION_TOGGLE_COPY_BUTTON,
        userDetails?.username,
        copyRedactionTextButton
      ),
      documentNameSearch: shouldShowFeature(
        FEATURE_FLAG_DOCUMENT_NAME_SEARCH,
        userDetails?.username,
        documentNameSearch,
        { groups: groupClaims, groupKey: PRIVATE_BETA_FEATURE_USER_GROUP6 }
      ),
    }),
    []
  );
  return useMemo(() => getFeatureFlags(), [getFeatureFlags]);
};
