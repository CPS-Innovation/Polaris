import { ApiError } from "../../../../common/errors/ApiError";
import { AsyncPipelineResult } from "./AsyncPipelineResult";
import { getPipelinePdfResults, initiatePipeline } from "../../api/gateway-api";
import { PipelineResults } from "../../domain/PipelineResults";
import { getPipelinpipelineCompletionStatus } from "../../domain/PipelineStatus";

const delay = (delayMs: number) =>
  new Promise((resolve) => setTimeout(resolve, delayMs));

export const initiateAndPoll = (
  // todo: _ wrap up in to an object arg
  urn: string,
  caseId: number,
  delayMs: number,
  del: (pipelineResults: AsyncPipelineResult<PipelineResults>) => void
) => {
  let keepPolling = true;
  let trackingCallCount = 0;

  const handleApiCallSuccess = (pipelineResult: PipelineResults) => {
    trackingCallCount += 1;

    const completionStatus = getPipelinpipelineCompletionStatus(
      pipelineResult.status
    );

    if (completionStatus === "Completed") {
      del({
        status: "complete",
        data: pipelineResult,
        haveData: true,
      });
      keepPolling = false;
    } else if (completionStatus === "Failed") {
      throw new Error(
        `Document processing pipeline returned with "Failed" status after ${trackingCallCount} polling attempts`
      );
    } else {
      del({
        status: "incomplete",
        data: pipelineResult,
        haveData: true,
      });
    }
  };

  const handleApiCallError = (error: any) => {
    keepPolling = false;
    del({
      status: "failed",
      error,
      httpStatusCode: error instanceof ApiError ? error.code : undefined,
      haveData: false,
    });
  };

  const doWork = async () => {
    let trackerArgs: Awaited<ReturnType<typeof initiatePipeline>>;

    try {
      trackerArgs = await initiatePipeline(urn, caseId);
    } catch (error) {
      handleApiCallError(error);
      return;
    }

    while (keepPolling) {
      try {
        await delay(delayMs);

        const pipelineResult = await getPipelinePdfResults(
          trackerArgs.trackerUrl,
          trackerArgs.correlationId
        );
        handleApiCallSuccess(pipelineResult);
      } catch (error) {
        handleApiCallError(error);
      }
    }
  };

  doWork();

  return () => {
    // allow consumer to kill loop
    keepPolling = false;
  };
};
