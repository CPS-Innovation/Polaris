import { useEffect } from "react";
import { useApi } from "../../../../common/hooks/useApi";
import {
  getRedactionLogLookUpsData,
  getRedactionLogMappingData,
} from "../../api/gateway-api";
import { DispatchType } from "./reducer";

// todo: these calls should be made once per app initialisation,
//  not once per case visit see #28170 for moving this out of here
export const useLoadAppLevelLookups = (
  dispatch: DispatchType,
  featureFlagRedactionLog: boolean
) => {
  // Load lookups
  const redactionLogLookUpsData = useApi(
    getRedactionLogLookUpsData,
    [],
    featureFlagRedactionLog
  );
  useEffect(() => {
    if (redactionLogLookUpsData.status !== "initial")
      dispatch({
        type: "UPDATE_REDACTION_LOG_LOOK_UPS_DATA",
        payload: redactionLogLookUpsData,
      });
  }, [redactionLogLookUpsData, dispatch]);

  // Load lookups
  const redactionLogMappingData = useApi(
    getRedactionLogMappingData,
    [],
    featureFlagRedactionLog
  );
  useEffect(() => {
    if (redactionLogMappingData.status !== "initial")
      dispatch({
        type: "UPDATE_REDACTION_LOG_MAPPING_DATA",
        payload: redactionLogMappingData,
      });
  }, [redactionLogMappingData, dispatch]);
};
