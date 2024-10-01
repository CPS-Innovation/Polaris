import { useEffect } from "react";
import { DispatchType } from "./reducer";
import {
  BACKGROUND_PIPELINE_REFRESH_INTERVAL_MS,
  FEATURE_BACKGROUND_PIPELINE_REFRESH,
} from "../../../../config";

export const usePipelineRefreshPolling = (
  intervalMs: number,
  dispatch: DispatchType
) =>
  useEffect(() => {
    const interval =
      FEATURE_BACKGROUND_PIPELINE_REFRESH &&
      setInterval(
        () =>
          dispatch({
            type: "UPDATE_REFRESH_PIPELINE",
            payload: { startRefresh: true },
          }),
        BACKGROUND_PIPELINE_REFRESH_INTERVAL_MS
      );
    return () => {
      interval && clearInterval(interval);
    };
  }, [intervalMs, dispatch]);
