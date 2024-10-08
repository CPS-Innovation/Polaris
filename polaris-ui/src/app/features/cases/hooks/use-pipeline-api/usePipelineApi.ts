import { useEffect, useState } from "react";
import { AsyncPipelineResult } from "./AsyncPipelineResult";
import { PipelineResults } from "../../domain/gateway/PipelineResults";
import { initiateAndPoll } from "./initiate-and-poll";
import { PIPELINE_POLLING_DELAY } from "../../../../config";
import { CombinedState } from "../../domain/CombinedState";
import { generateGuid } from "../../../cases/api/generate-guid";

export const usePipelineApi = (
  urn: string,
  caseId: number,
  pipelineRefreshData: CombinedState["pipelineRefreshData"],
  isUnMounting: () => boolean
): {
  pipelineResults: AsyncPipelineResult<PipelineResults>;
  pipelineBusy: boolean;
} => {
  const [pipelineResults, setPipelineResults] = useState<
    AsyncPipelineResult<PipelineResults>
  >({
    status: "initiating",
    haveData: false,
    correlationId: "",
  });

  const [pipelineBusy, setPipelineBusy] = useState(false);

  useEffect(() => {
    if (
      // the outside world is calling for a refresh...
      pipelineRefreshData.startRefresh &&
      // ... and we are not already doing a refresh
      !pipelineBusy
    ) {
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
  }, [caseId, urn, pipelineRefreshData, pipelineBusy, isUnMounting]);

  useEffect(() => {
    if (pipelineResults.status !== "initiating") {
      setPipelineBusy(false);
    }
  }, [pipelineResults]);

  return { pipelineResults, pipelineBusy };
};
