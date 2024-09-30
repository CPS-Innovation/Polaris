import { DispatchType } from "./reducer";

export const usePipelineRefreshPolling = (
  intervalMs: number,
  dispatch: DispatchType
) => {
  if (intervalMs <= 0) {
    return null;
  }

  const interval = setInterval(() => {
    dispatch({ type: "UPDATE_BACKGROUND_REFRESH_PIPELINE" });
  }, intervalMs);

  return () => clearInterval(interval);
};
