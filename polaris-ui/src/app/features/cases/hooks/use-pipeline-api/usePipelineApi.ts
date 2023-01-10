import { useEffect, useState } from "react";
import { AsyncPipelineResult } from "./AsyncPipelineResult";
import { PipelineResults } from "../../domain/PipelineResults";
import { initiateAndPoll } from "./initiate-and-poll";
import { PIPELINE_POLLING_DELAY } from "../../../../config";

export const usePipelineApi = (
  urn: string,
  caseId: number
): AsyncPipelineResult<PipelineResults> => {
  const [pipelineResults, setPipelineResults] = useState<
    AsyncPipelineResult<PipelineResults>
  >({
    status: "initiating",
    haveData: false,
  });

  useEffect(() => {
    return initiateAndPoll(urn, caseId, PIPELINE_POLLING_DELAY, (results) =>
      setPipelineResults(results)
    );
  }, [caseId, urn]);

  return pipelineResults;
};
