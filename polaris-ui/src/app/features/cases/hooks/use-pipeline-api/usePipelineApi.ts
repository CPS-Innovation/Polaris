import { useEffect, useState } from "react";
import { AsyncPipelineResult } from "./AsyncPipelineResult";
import { PipelineResults } from "../../domain/gateway/PipelineResults";
import { initiateAndPoll } from "./initiate-and-poll";
import { PIPELINE_POLLING_DELAY } from "../../../../config";
import { CombinedState } from "../../domain/CombinedState";

export const usePipelineApi = (
  urn: string,
  caseId: number,
  pipelineRefreshData: CombinedState["pipelineRefreshData"]
): AsyncPipelineResult<PipelineResults> => {
  const [pipelineResults, setPipelineResults] = useState<
    AsyncPipelineResult<PipelineResults>
  >({
    status: "initiating",
    haveData: false,
  });

  useEffect(() => {
    if (pipelineRefreshData.startRefresh) {
      initiateAndPoll(
        urn,
        caseId,
        PIPELINE_POLLING_DELAY,
        pipelineRefreshData,
        (results) => setPipelineResults(results)
      );
    }
  }, [caseId, urn, pipelineRefreshData]);

  return pipelineResults;
};
