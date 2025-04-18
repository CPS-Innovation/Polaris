import { useEffect, useState, useMemo } from "react";
import { AsyncPipelineResult } from "./AsyncPipelineResult";
import { PipelineResults } from "../../domain/gateway/PipelineResults";
import { initiateAndPoll } from "./initiate-and-poll";
import { PIPELINE_POLLING_DELAY } from "../../../../config";
import { CombinedState } from "../../domain/CombinedState";
import { generateGuid } from "../../../cases/api/generate-guid";
import { DispatchType } from "../use-case-details-state/reducer";
import { shouldTriggerPipelineRefresh } from "../utils/shouldTriggerPipelineRefresh";

export const usePipelineApi = (
  urn: string,
  caseId: number,
  pipelineRefreshData: CombinedState["pipelineRefreshData"],
  lastModifiedDateTime: string | undefined,
  isUnMounting: () => boolean,
  dispatch: DispatchType
) => {
  const [pipelineResults, setPipelineResults] =
    useState<AsyncPipelineResult<PipelineResults> | null>(null);

  const [pipelineBusy, setPipelineBusy] = useState(false);

  const triggerPipelineRefresh = useMemo(() => {
    return shouldTriggerPipelineRefresh(
      lastModifiedDateTime ?? "",
      pipelineRefreshData.localLastRefreshTime
    );
  }, [lastModifiedDateTime, pipelineRefreshData.localLastRefreshTime]);

  useEffect(() => {
    if (
      // the outside world is calling for a refresh...
      pipelineRefreshData.startPipelineRefresh &&
      triggerPipelineRefresh &&
      // ... and we are not already doing a refresh
      !pipelineBusy
    ) {
      setPipelineResults({
        status: "initiating",
        correlationId: "",
      });
      const correlationId = generateGuid();
      //get correlationID here and add it to the setPipelineResults and remove it from gateway
      initiateAndPoll(
        urn,
        caseId,
        PIPELINE_POLLING_DELAY,
        pipelineRefreshData,
        correlationId,
        (results) => setPipelineResults(results),
        isUnMounting
      );
      setPipelineBusy(true);
    }
  }, [
    caseId,
    urn,
    pipelineRefreshData,
    isUnMounting,
    pipelineBusy,
    triggerPipelineRefresh,
    setPipelineResults,
  ]);

  useEffect(() => {
    if (
      pipelineResults?.status === "failed" ||
      pipelineResults?.status === "complete"
    ) {
      setPipelineBusy(false);
    }
  }, [pipelineResults]);

  useEffect(() => {
    if (pipelineResults)
      dispatch({
        type: "UPDATE_PIPELINE",
        payload: pipelineResults,
      });
  }, [pipelineResults, dispatch]);
};
