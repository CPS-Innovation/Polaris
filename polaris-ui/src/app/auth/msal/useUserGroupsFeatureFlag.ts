import { msalInstance } from "./msalInstance";
import {
  FEATURE_FLAG_REDACTION_LOG,
  PRIVATE_BETA_CHECK_IGNORE_USER,
  FEATURE_FLAG_FULL_SCREEN,
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

export const useUserGroupsFeatureFlag = (): FeatureFlagData => {
  const { redactionLog, fullScreen } =
    useQueryParamsState<FeatureFlagQueryParams>();
  const [account] = msalInstance.getAllAccounts();
  const userDetails = useUserDetails();
  const groupClaims = account?.idTokenClaims?.groups as string[];

  return {
    redactionLog: showRedactionLogFeature(userDetails?.username, redactionLog),
    fullScreen: showFullScreenFeature(userDetails?.username, fullScreen),
  };
};
