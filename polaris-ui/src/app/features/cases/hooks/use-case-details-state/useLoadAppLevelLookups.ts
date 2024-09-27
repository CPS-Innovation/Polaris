import { useEffect } from "react";
import { useUserGroupsFeatureFlag } from "../../../../auth/msal/useUserGroupsFeatureFlag";
import { useApi } from "../../../../common/hooks/useApi";
import {
  getRedactionLogLookUpsData,
  getRedactionLogMappingData,
} from "../../api/gateway-api";
import { DispatchType } from "./reducer";

export const useLoadAppLevelLookups = (dispatch: DispatchType) => {
  // Read feature flags in from MSAL-world
  const featureFlagData = useUserGroupsFeatureFlag();
  useEffect(() => {
    dispatch({
      type: "UPDATE_FEATURE_FLAGS_DATA",
      payload: featureFlagData,
    });
  }, [featureFlagData, dispatch]);

  // Load lookups - see #28170 for moving this out of here
  const redactionLogLookUpsData = useApi(
    getRedactionLogLookUpsData,
    [],
    featureFlagData.redactionLog
  );
  useEffect(() => {
    if (redactionLogLookUpsData.status !== "initial")
      dispatch({
        type: "UPDATE_REDACTION_LOG_LOOK_UPS_DATA",
        payload: redactionLogLookUpsData,
      });
  }, [redactionLogLookUpsData, dispatch]);

  // Load lookups - see #28170 for moving this out of here
  const redactionLogMappingData = useApi(
    getRedactionLogMappingData,
    [],
    featureFlagData.redactionLog
  );
  useEffect(() => {
    if (redactionLogMappingData.status !== "initial")
      dispatch({
        type: "UPDATE_REDACTION_LOG_MAPPING_DATA",
        payload: redactionLogMappingData,
      });
  }, [redactionLogMappingData, dispatch]);
};
