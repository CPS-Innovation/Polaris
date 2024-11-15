import { useEffect, useState, useCallback } from "react";
import { AsyncPipelineResult } from "./AsyncPipelineResult";
import { PipelineResults } from "../../domain/gateway/PipelineResults";
import { initiateAndPoll } from "./initiate-and-poll";
import { PIPELINE_POLLING_DELAY } from "../../../../config";
import { CombinedState } from "../../domain/CombinedState";
import { generateGuid } from "../../../cases/api/generate-guid";
import { DispatchType } from "../use-case-details-state/reducer";

export const usePipelineApi = (
  urn: string,
  caseId: number,
  pipelineRefreshData: CombinedState["pipelineRefreshData"],
  isUnMounting: () => boolean,
  dispatch: DispatchType
) => {
  const [pipelineResults, setPipelineResults] = useState<
    AsyncPipelineResult<PipelineResults>
  >({
    status: "initiating",
    correlationId: "",
  });

  const [pipelineBusy, setPipelineBusy] = useState(false);

  const shouldTriggerPipelineRefresh = useCallback(() => {
    return true; //Note here we should compare with the last modified time
  }, [pipelineResults?.status]);

  useEffect(() => {
    if (
      // the outside world is calling for a refresh...
      pipelineRefreshData.startPipelineRefresh &&
      shouldTriggerPipelineRefresh() &&
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
    pipelineBusy,
    isUnMounting,
    shouldTriggerPipelineRefresh,
  ]);

  useEffect(() => {
    if (pipelineResults.status !== "initiating") {
      setPipelineBusy(false);
    }
  }, [pipelineResults]);

  useEffect(() => {
    console.log("update pipeline....", pipelineResults);
    dispatch({
      type: "UPDATE_PIPELINE",
      payload: pipelineResults,
    });
  }, [pipelineResults, dispatch]);
};
