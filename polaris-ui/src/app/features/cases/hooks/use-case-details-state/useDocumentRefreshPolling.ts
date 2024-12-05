import { useEffect } from "react";
import { DispatchType } from "./reducer";
import { BACKGROUND_PIPELINE_REFRESH_INTERVAL_MS } from "../../../../config";

export const useDocumentRefreshPolling = (
  dispatch: DispatchType,
  isFeatureFlagOn: boolean
) =>
  useEffect(() => {
    const interval =
      isFeatureFlagOn &&
      setInterval(() => {
        dispatch({
          type: "UPDATE_DOCUMENT_REFRESH",
          payload: { startDocumentRefresh: true },
        });
      }, BACKGROUND_PIPELINE_REFRESH_INTERVAL_MS);
    return () => {
      interval && clearInterval(interval);
    };
  }, [dispatch, isFeatureFlagOn]);
