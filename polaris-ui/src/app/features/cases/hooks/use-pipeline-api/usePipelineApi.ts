import { useEffect, useState } from "react";
import { AsyncPipelineResult } from "./AsyncPipelineResult";
import { PipelineResults } from "../../domain/PipelineResults";
import { initiateAndPoll } from "./initiate-and-poll";
import { PIPELINE_POLLING_DELAY } from "../../../../config";
import { CombinedState } from "../../domain/CombinedState";

export const usePipelineApi = (
  urn: string,
  caseId: number,
  generalPipelineState: CombinedState["generalPipelineState"]
): AsyncPipelineResult<PipelineResults> => {
  const { refreshData } = generalPipelineState;
  const [pipelineResults, setPipelineResults] = useState<
    AsyncPipelineResult<PipelineResults>
  >({
    status: "initiating",
    haveData: false,
  });

  useEffect(() => {
    if (refreshData.startRefresh) {
      console.log("initiateAndPoll >>>>>");
      initiateAndPoll(
        urn,
        caseId,
        PIPELINE_POLLING_DELAY,
        generalPipelineState,
        (results) => setPipelineResults(results)
      );
    }
  }, [caseId, urn, refreshData.startRefresh, generalPipelineState]);

  return pipelineResults;
};
