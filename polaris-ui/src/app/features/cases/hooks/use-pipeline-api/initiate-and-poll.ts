import { ApiError } from "../../../../common/errors/ApiError";
import { AsyncPipelineResult } from "./AsyncPipelineResult";
import { getPipelinePdfResults, initiatePipeline } from "../../api/gateway-api";
import { PipelineResults } from "../../domain/gateway/PipelineResults";
import { getPipelineCompletionStatus } from "../../domain/gateway/PipelineStatus";
import { CombinedState } from "../../domain/CombinedState";
import { isNewTime } from "../utils/refreshUtils";
const delay = (delayMs: number) =>
  new Promise((resolve) => setTimeout(resolve, delayMs));

export const initiateAndPoll = (
  urn: string,
  caseId: number,
  delayMs: number,
  pipelineRefreshData: CombinedState["pipelineRefreshData"],
  correlationId: string,
  del: (pipelineResults: AsyncPipelineResult<PipelineResults>) => void,
  shouldStopPolling: () => boolean
) => {
  let keepPolling = true;
  let trackingCallCount = 0;

  const { lastProcessingCompleted } = pipelineRefreshData;

  const handleApiCallSuccess = (pipelineResult: PipelineResults) => {
    trackingCallCount += 1;

    const completionStatus = getPipelineCompletionStatus(pipelineResult.status);
    if (
      completionStatus === "Completed" &&
      isNewTime(pipelineResult.processingCompleted, lastProcessingCompleted)
    ) {
      del({
        status: "complete",
        data: pipelineResult,
        correlationId,
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
        correlationId,
      });
    }
  };

  const handleApiCallError = (error: any) => {
    keepPolling = false;
    del({
      status: "failed",
      error,
      httpStatusCode: error instanceof ApiError ? error.code : undefined,
      correlationId,
    });
  };

  const startInitiatePipelinePolling = async () => {
    // First we poll the kick-off endpoint until this request is accepted (we poll because
    // we may already have a refresh in-flight.

    // Note: `while (true)` plus using `break` is not enough. We need to be able to cancel
    //  polling from the consumer, hence
    while (keepPolling) {
      try {
        await delay(delayMs);
        if (shouldStopPolling()) {
          keepPolling = false;
          break;
        }
        const trackerArgs = await initiatePipeline(urn, caseId, correlationId);
        startTrackerPolling(trackerArgs);
        break;
      } catch (error) {
        handleApiCallError(error);
      }
    }
  };

  const startTrackerPolling = async (
    trackerArgs: Awaited<ReturnType<typeof initiatePipeline>>
  ) => {
    while (keepPolling) {
      try {
        await delay(delayMs);
        if (shouldStopPolling()) {
          keepPolling = false;
          break;
        }

        const [pipelineResults] = await Promise.all([
          getPipelinePdfResults(
            trackerArgs.trackerUrl,
            trackerArgs.correlationId
          ),
        ]);

        if (pipelineResults) {
          handleApiCallSuccess(pipelineResults);
        }
      } catch (error) {
        handleApiCallError(error);
      }
    }
  };

  const doWork = () => {
    startInitiatePipelinePolling();
  };
  doWork();

  return () => {
    // allow consumer to kill loop
    keepPolling = false;
  };
};
