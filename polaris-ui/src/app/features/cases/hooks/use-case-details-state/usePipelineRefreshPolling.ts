import { useEffect } from "react";
import { DispatchType } from "./reducer";
import { BACKGROUND_PIPELINE_REFRESH_INTERVAL_MS } from "../../../../config";

export const usePipelineRefreshPolling = (
  intervalMs: number,
  dispatch: DispatchType,
  isFeatureFlagOn: boolean
) =>
  useEffect(() => {
    const interval =
      isFeatureFlagOn &&
      setInterval(() => {
        dispatch({
          type: "UPDATE_REFRESH_PIPELINE",
          payload: { startRefresh: true },
        });
      }, BACKGROUND_PIPELINE_REFRESH_INTERVAL_MS);
    return () => {
      interval && clearInterval(interval);
    };
  }, [intervalMs, dispatch, isFeatureFlagOn]);
