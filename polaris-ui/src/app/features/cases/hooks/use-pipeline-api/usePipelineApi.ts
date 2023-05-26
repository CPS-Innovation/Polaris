import { useEffect, useState } from "react";
import { AsyncPipelineResult } from "./AsyncPipelineResult";
import { PipelineResults } from "../../domain/gateway/PipelineResults";
import { initiateAndPoll } from "./initiate-and-poll";
import { PIPELINE_POLLING_DELAY } from "../../../../config";
import { CombinedState } from "../../domain/CombinedState";
import * as HEADERS from "../../../cases/api/header-factory";

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
    correlationId: "",
  });

  useEffect(() => {
    if (pipelineRefreshData.startRefresh) {
      const correlationId = HEADERS.correlationId()["Correlation-Id"];
      //get correlationID here and add it ot the setPipelineResults and remove it from gateway
      initiateAndPoll(
        urn,
        caseId,
        PIPELINE_POLLING_DELAY,
        pipelineRefreshData,
        correlationId,
        (results) => setPipelineResults(results)
      );
    }
  }, [caseId, urn, pipelineRefreshData]);

  return pipelineResults;
};
